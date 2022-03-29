using Devdeb.Network.TCP.Communication;
using System;

namespace Devdeb.Audio.InternetTelephony.Server.Models
{
	internal class User
	{
		public Guid Id { get; set; }
		public TcpCommunication TcpCommunication { get; set; }
		public string Name { get; set; }
	}
}
