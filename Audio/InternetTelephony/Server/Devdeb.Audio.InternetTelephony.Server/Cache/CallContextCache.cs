using Devdeb.Audio.InternetTelephony.Server.Models;
using Devdeb.Network.TCP.Communication;
using System.Collections.Generic;
using System.Linq;

namespace Devdeb.Audio.InternetTelephony.Server.Cache
{
	internal class CallContextCache
	{
		private readonly List<CallContext> _callContexts;

		public CallContextCache()
		{
			_callContexts = new List<CallContext>();
		}

		public void Add(CallContext callContext) => _callContexts.Add(callContext);

		public CallContext GetByTcpCommunication(TcpCommunication tcpCommunication)
		{ 
			return _callContexts.First(x => 
				x.Caller.TcpCommunication == tcpCommunication || 
				x.Called.TcpCommunication == tcpCommunication
			);
		}
	}
}
