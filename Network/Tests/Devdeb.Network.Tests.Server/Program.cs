using Devdeb.Network.Tests.Server;

namespace Devdeb.Tests.Network.Server
{
    class Program
    {
        private static readonly BaseTcpServerTest _baseTcpServerTest;
        private static readonly TcpCommunicationTest _tcpCommunicationTest;
        private static readonly DefaultTest _defaultTest;

        static Program()
        {
            _baseTcpServerTest = new BaseTcpServerTest();
            _tcpCommunicationTest = new TcpCommunicationTest();
            _defaultTest = new DefaultTest();
        }

        static void Main(string[] args) => _baseTcpServerTest.Test();
    }
}