using Devdeb.Audio.InternetTelephony.Contracts.Models.Calls;
using System.Threading.Tasks;

namespace Devdeb.Audio.InternetTelephony.Client.Services
{
	internal class CallAcceptanceRequestor
	{
		private TaskCompletionSource<CallAcceptanceResponse> _registerResponce;

		public CallAcceptanceRequest AcceptanceRequest { get; private set; }

		public void SetAcceptanceResponse(CallAcceptanceResponse response)
		{
			_registerResponce.TrySetResult(response);
			_registerResponce = null;
			AcceptanceRequest = null;
		}

		public Task<CallAcceptanceResponse> RegisterCallAcceptanceRequest(CallAcceptanceRequest request)
		{
			AcceptanceRequest = request;
			_registerResponce = new TaskCompletionSource<CallAcceptanceResponse>();
			return _registerResponce.Task;
		}
	}
}
