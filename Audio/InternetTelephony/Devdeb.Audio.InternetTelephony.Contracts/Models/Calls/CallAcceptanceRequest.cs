using System;

namespace Devdeb.Audio.InternetTelephony.Contracts.Models.Calls
{
	public class CallAcceptanceRequest
	{
		public Guid CallId { get; set; }
		public string UserName { get; set; }
	}
}
