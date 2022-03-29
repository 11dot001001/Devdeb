using Devdeb.Network.TCP.Communication;
using System;

namespace Devdeb.Audio.InternetTelephony.Server.Models
{
	internal class CallContext
	{
		internal class LinePoint
		{
			public Guid UserId { get; set; }
			public TcpCommunication TcpCommunication { get; set; }
		}

		public LinePoint Caller { get; set; }
		public LinePoint Called { get; set; }
	}
}
