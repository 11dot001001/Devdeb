using Devdeb.DependencyInjection;
using Devdeb.Network.TCP.Communication;
using Devdeb.Network.TCP.Rpc.Requestor;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Devdeb.Network.TCP.Rpc.Connections
{
	public class Connection
	{
		public class ProcessingContext
		{ 
			public IServiceProvider ServiceProvider { get; set; }
			public Task ProcessingTask { get; set; }
		}

		private readonly TcpCommunication _tcpCommunication;
		private readonly RequestorCollection _requestorCollection;
		private readonly HashSet<ProcessingContext> _processingContexts; //check concurrency
		
		public Connection(TcpCommunication tcpCommunication, RequestorCollection requestorCollection)
		{
			_tcpCommunication = tcpCommunication ?? throw new System.ArgumentNullException(nameof(tcpCommunication));
			_requestorCollection = requestorCollection ?? throw new System.ArgumentNullException(nameof(requestorCollection));
			_processingContexts = new HashSet<ProcessingContext>();
		}

		public TcpCommunication TcpCommunication => _tcpCommunication;
		public RequestorCollection RequestorCollection => _requestorCollection;
		public HashSet<ProcessingContext> ProcessingContexts => _processingContexts;

		public void Close()
		{
			_tcpCommunication.Close();
			// stop all client handlers and other...
		}
	}
}
