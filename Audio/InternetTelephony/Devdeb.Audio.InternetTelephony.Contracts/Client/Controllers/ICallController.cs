using Devdeb.Audio.InternetTelephony.Contracts.Models.Calls;
using System.Threading.Tasks;

namespace Devdeb.Audio.InternetTelephony.Contracts.Client.Controllers
{
	public interface ICallController
	{
		Task<CallAcceptanceResponse> RequestCallAcceptance(CallAcceptanceRequest callAcceptanceRequest);
		void PlayPcm(byte[] buffer);
	}
}
