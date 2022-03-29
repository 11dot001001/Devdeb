using Devdeb.Audio.InternetTelephony.Contracts.Models.Calls;
using System;
using System.Threading.Tasks;

namespace Devdeb.Audio.InternetTelephony.Contracts.Server.Controllers
{
	public interface ICallController
	{ 
		Task<StartCallResponse> StartCall(Guid userId);

		void SendPcm(byte[] buffer);
	}
}
