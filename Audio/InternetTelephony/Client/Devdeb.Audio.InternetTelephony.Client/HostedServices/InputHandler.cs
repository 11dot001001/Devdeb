using Devdeb.Audio.InternetTelephony.Client.Services;
using Devdeb.Audio.InternetTelephony.Contracts.Models.Calls;
using Devdeb.Audio.InternetTelephony.Contracts.Models.Users;
using Devdeb.Audio.InternetTelephony.Contracts.Server;
using Devdeb.Network.TCP.Rpc.HostedServices;
using NAudio.Wave;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Devdeb.Audio.InternetTelephony.Client.HostedServices
{
	internal class InputHandler : IHostedService
	{
		private class CallInfo
		{
			public Guid CallId { get; set; }
			public string UserName { get; set; }
		}

		private readonly ServerApi _serverApi;
		private readonly CallAcceptanceRequestor _callAcceptanceRequestor;
		private CallInfo _currentCall;

		public InputHandler(ServerApi serverApi, CallAcceptanceRequestor callAcceptanceRequestor)
		{
			_serverApi = serverApi ?? throw new ArgumentNullException(nameof(serverApi));
			_callAcceptanceRequestor = callAcceptanceRequestor ?? throw new ArgumentNullException(nameof(callAcceptanceRequestor));
		}

		public async Task StartAsync()
		{
			Console.WriteLine("Hello! Input your name:");
			string name = Console.ReadLine();

			var createUserResponse = await _serverApi
				.UserController
				.CreateUser(new CreateUserRequest
				{
					Name = name
				});

			if (!createUserResponse.IsSuccessed)
			{
				Console.WriteLine("Create user faulted.");
				return;
			}

			for (; ; await Task.Delay(2000))
			{
				Console.Clear();
				if (_currentCall != null)
					HandleCurrentCall();
				else if (_callAcceptanceRequestor.AcceptanceRequest != null)
					HandleCallAcceptanceRequst();
				else
					await HandleDefaultScenario();
			}
		}
		private void HandleCurrentCall()
		{
			const int _sampleRate = 48000;
			const int _bitsPerSample = 16;
			const int _channel = 2;

			Console.WriteLine($"Speak with {_currentCall.UserName}...");
			using var inputDevice = new WaveInEvent();
			inputDevice.WaveFormat = new WaveFormat(_sampleRate, _bitsPerSample, _channel);

			inputDevice.DataAvailable += InputDevice_DataAvailable; ;
			inputDevice.StartRecording();
			for (; ; )
				Task.Delay(10000);
		}

		private void InputDevice_DataAvailable(object sender, WaveInEventArgs e)
		{
			_serverApi.CallController.SendPcm(e.Buffer.AsSpan()[..e.BytesRecorded].ToArray());
		}

		private void HandleCallAcceptanceRequst()
		{
			var acceptanceRequest = _callAcceptanceRequestor.AcceptanceRequest;
			Console.WriteLine($"User {acceptanceRequest.UserName} try call you.");
			for (; ; )
			{
				Console.WriteLine($"Write Y to accept; N - decline.");
				string result = Console.ReadLine();
				if (result == "Y")
				{
					_callAcceptanceRequestor.SetAcceptanceResponse(new CallAcceptanceResponse() { IsAccepted = true });
					_currentCall = new CallInfo()
					{
						CallId = acceptanceRequest.CallId,
						UserName = acceptanceRequest.UserName
					};
					break;
				}
				else if (result == "N")
				{
					_callAcceptanceRequestor.SetAcceptanceResponse(new CallAcceptanceResponse() { IsAccepted = false });
					break;
				}
			}
		}
		private async Task HandleDefaultScenario()
		{
			var activeUsers = await _serverApi.UserController.GetActiveUsers();
			if (!activeUsers.Any())
			{
				Console.WriteLine("No one active users here.");
				return;
			}
			Console.WriteLine($"Active users: {string.Join('\n', activeUsers.Select(x => x.Name))}");
			Console.Write("Write the name to whom we will call: ");
			string chosenName = Console.ReadLine();
			if (_callAcceptanceRequestor.AcceptanceRequest != null)
			{
				Console.WriteLine("Someone is calling you");
				return;
			}

			ListItemUserVm chosenUser = activeUsers.FirstOrDefault(x => x.Name == chosenName);
			if (chosenUser == null)
			{
				Console.WriteLine($"Ivalid name \"{chosenName}\"");
				return;
			}

			Console.WriteLine("Calling...");
			var callRequest = await _serverApi.CallController.StartCall(chosenUser.Id);
			if (!callRequest.IsAccepted)
			{
				Console.WriteLine("Call rejected");
				return;
			}
			_currentCall = new CallInfo()
			{
				CallId = callRequest.CallId.Value,
				UserName = chosenName
			};
		}


		public Task StopAsync() => throw new NotImplementedException();
	}
}
