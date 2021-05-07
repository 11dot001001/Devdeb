using System;
using System.Net.Sockets;
using System.Net;
using Devdeb.Network.TCP.Communication;

namespace Devdeb.Network.Tests.Client
{
    public class TcpCommunicationTest
    {
        static private readonly IPAddress _iPAddress = IPAddress.Parse("127.0.0.1");
        static private readonly int _port = 25000;

        public void Test()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            bool connected = false;
            Exception[] exceptions = new Exception[4];
            try
            {
                for (int connectionTry = 0; !connected; connectionTry++)
                {
                    if (connectionTry == 4)
                        throw new AggregateException("The connection attempts exceeds max available count.", exceptions);
                    try
                    {
                        socket.Connect(new IPEndPoint(_iPAddress, _port));
                        connected = true;
                    }
                    catch (SocketException socketException)
                    {
                        exceptions[connectionTry] = socketException;
                        if (socketException.SocketErrorCode != SocketError.ConnectionRefused)
                            throw socketException;
                        Console.WriteLine($"Connection try {connectionTry}. " + socketException.Message);
                    }
                }
            }
            catch (Exception ex)
            {

            }


            TcpCommunication communication = new TcpCommunication(socket);

            long totalSent = 0;
            byte[] buffer = new byte[30000];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = 1;
            }

            int sendCount = new Random().Next();

            for (; ; )
            {
                communication.SendBuffer();
                if (totalSent < sendCount)
                {
                    int localSendCount = new Random().Next(1, buffer.Length);
                    communication.Send(buffer, 0, localSendCount);
                    totalSent += localSendCount;
                    Console.WriteLine(totalSent);
                }
            }
        }
    }
}
