using System;

namespace Devdeb.Audio.InternetTelephony.Contracts.Models.Calls
{
	public class StartCallResponse
	{
		public bool IsAccepted { get; set; }
		public Guid? CallId { get; set; }
	}
}
