using Devdeb.Audio.InternetTelephony.Contracts.Client;
using Devdeb.Audio.InternetTelephony.Contracts.Models.Calls;
using Devdeb.Audio.InternetTelephony.Contracts.Server.Controllers;
using Devdeb.Audio.InternetTelephony.Server.Cache;
using Devdeb.Audio.InternetTelephony.Server.Models;
using Devdeb.Network.TCP.Communication;
using Devdeb.Network.TCP.Rpc.Connections;
using Devdeb.Network.TCP.Rpc.Requestor.Context;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Devdeb.Audio.InternetTelephony.Server.Controllers
{
	internal class CallController : ICallController
	{
		private readonly UserCache _userCache;
		private readonly CallContextCache _callContextCache;
		private readonly IConnectionStorage _connectionStorage;
		private readonly IRequestorContext _requestorContext;

		public CallController(UserCache userCache, CallContextCache callContextCache, IConnectionStorage connectionStorage, IRequestorContext requestorContext)
		{
			_userCache = userCache ?? throw new ArgumentNullException(nameof(userCache));
			_callContextCache = callContextCache ?? throw new ArgumentNullException(nameof(callContextCache));
			_connectionStorage = connectionStorage ?? throw new ArgumentNullException(nameof(connectionStorage));
			_requestorContext = requestorContext ?? throw new ArgumentNullException(nameof(requestorContext));
		}

		public async Task<StartCallResponse> StartCall(Guid userId)
		{
			if (!_userCache.TryGetUser(userId, out var user))
				return new StartCallResponse() { IsAccepted = false };

			var targetUserConnection = _connectionStorage.Get(user.TcpCommunication);
			if (targetUserConnection == null)
				return new StartCallResponse() { IsAccepted = false };

			var currentUser = _userCache.Users.First(x => x.TcpCommunication == _requestorContext.TcpCommunication);

			Guid callId = Guid.NewGuid();

			var targetUserClientApi = (ClientApi)targetUserConnection.RequestorCollection;
			var acceptanceResponce = await targetUserClientApi
											.CallController
											.RequestCallAcceptance(
												new CallAcceptanceRequest
												{
													CallId = callId,
													UserName = currentUser.Name
												}
											);

			if (!acceptanceResponce.IsAccepted)
				return new StartCallResponse() { IsAccepted = false };

			Console.WriteLine($"Register dialogs between {currentUser.Id} and {userId}");

			_callContextCache.Add(new CallContext
			{
				Caller = new CallContext.LinePoint
				{
					UserId = currentUser.Id,
					TcpCommunication = _requestorContext.TcpCommunication
				},
				Called = new CallContext.LinePoint
				{ 
					UserId = userId,
					TcpCommunication = targetUserConnection.TcpCommunication
				}
			});

			return new StartCallResponse() { IsAccepted = true, CallId = callId };
		}

		public void SendPcm(byte[] buffer)
		{
			var callContext = _callContextCache.GetByTcpCommunication(_requestorContext.TcpCommunication);

			TcpCommunication targetUser = callContext.Called.TcpCommunication == _requestorContext.TcpCommunication
				? callContext.Caller.TcpCommunication
				: callContext.Called.TcpCommunication;

			Connection targetUserConnection = _connectionStorage.Get(targetUser);

			var targetUserClientApi = (ClientApi)targetUserConnection.RequestorCollection;
			targetUserClientApi.CallController.PlayPcm(buffer);
		}
	}
}
