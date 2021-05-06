namespace Devdeb.Network.Tests.Client
{
    class Program
    {
        private static readonly BaseTcpClientTest _baseTcpClientTest;
        private static readonly TcpCommunicationTest _tcpCommunicationTest;
        private static readonly DefaultTest _defaultTest;

        static Program()
        {
            _baseTcpClientTest = new BaseTcpClientTest();
            _tcpCommunicationTest = new TcpCommunicationTest();
            _defaultTest = new DefaultTest();
        }

        static void Main(string[] args) => _baseTcpClientTest.Test();
    }
}
