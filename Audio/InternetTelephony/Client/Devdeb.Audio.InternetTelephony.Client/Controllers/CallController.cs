using Devdeb.Audio.InternetTelephony.Client.Audio;
using Devdeb.Audio.InternetTelephony.Client.Services;
using Devdeb.Audio.InternetTelephony.Contracts.Client.Controllers;
using Devdeb.Audio.InternetTelephony.Contracts.Models.Calls;
using System;
using System.Threading.Tasks;

namespace Devdeb.Audio.InternetTelephony.Client.Controllers
{
	internal class CallController : ICallController
	{
		private readonly AudioPlayer _audioPlayer;
		private readonly CallAcceptanceRequestor _callAcceptanceRequestor;

		public CallController(AudioPlayer audioPlayer, CallAcceptanceRequestor callAcceptanceRequestor)
		{
			_audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer));
			_callAcceptanceRequestor = callAcceptanceRequestor ?? throw new ArgumentNullException(nameof(callAcceptanceRequestor));
		}

		public Task<CallAcceptanceResponse> RequestCallAcceptance(CallAcceptanceRequest callAcceptanceRequest)
		{
			return _callAcceptanceRequestor.RegisterCallAcceptanceRequest(callAcceptanceRequest);
		}

		public void PlayPcm(byte[] buffer) => _audioPlayer.AddBufferToPlay(buffer);
	}
}
