using Devdeb.Network.Tests.Server;
using System;
using System.Text;

namespace Devdeb.Tests.Network.Server
{
    class Program
    {
        private static readonly BaseTcpTest _baseTcpTest;
        private static readonly TcpCommunicationTest _tcpCommunicationTest;
        private static readonly DefaultTest _defaultTest;
        private static readonly ExpectingTcpTest _expectingTcpTest;

        static Program()
        {
            Console.OutputEncoding = Encoding.UTF8;
            _baseTcpTest = new BaseTcpTest();
            _tcpCommunicationTest = new TcpCommunicationTest();
            _defaultTest = new DefaultTest();
            _expectingTcpTest = new ExpectingTcpTest();
        }

        static void Main(string[] args) { }
    }
}