using Devdeb.Network.TCP.Communication;
using Devdeb.Network.TCP.Expecting;
using Devdeb.Serialization;
using Devdeb.Serialization.Default;
using Devdeb.Serialization.Serializers;
using Devdeb.Serialization.Serializers.System;
using System;
using System.Net;

namespace Devdeb.Network.Tests.Client
{
    public class RpcTest
    {
        static private readonly IPAddress _iPAddress = IPAddress.Parse("127.0.0.1");
        static private readonly int _port = 25000;

        public void Test()
        {
            RpcClient rpcClient = new RpcClient(_iPAddress, _port);
            rpcClient.Start();
            Guid id = rpcClient.Requestor.AddStudent(
                new StudentFm
                {
                    Name = "Серафим Студентович",
                    Age = 20
                },
                10
            );
            rpcClient.Requestor.DeleteStudent(Guid.Parse("6a6ccb67-df1e-478e-b649-311a9f9ec2db"));
            rpcClient.Requestor.DeleteStudent(Guid.Parse("6a6ccb67-df1e-478e-b649-311a9f9ec2db"));
            rpcClient.Requestor.DeleteStudent(Guid.Parse("6a6ccb67-df1e-478e-b649-311a9f9ec2db"));
            Console.ReadKey();
        }

        public abstract class BaseRpcClient<TServer, TClient> : BaseExpectingTcpClient
        {
            private readonly TServer _requestor;

            public BaseRpcClient(IPAddress iPAddress, int port) : base(iPAddress, port)
            {
                _requestor = BuildRequestor();
            }

            private TServer BuildRequestor()
            {
                Type serverInterfaceType = typeof(TServer);
                return default;
            }

            public TServer Requestor => _requestor;

            protected override void ProcessCommunication(TcpCommunication tcpCommunication, int count)
            {
                string message = tcpCommunication.Receive(StringLengthSerializer.UTF8, count);
                Console.WriteLine($"{tcpCommunication.Socket.RemoteEndPoint} message: {message}.");
            }
        }

        public class RpcClient : BaseRpcClient<IServer, IClient>, IClient
        {
            public new IServer Requestor;

            public RpcClient(IPAddress iPAddress, int port) : base(iPAddress, port)
            { }

            protected override void NotifyStarted()
            {
                Requestor = new ServerRequestor(TcpCommunication);
            }

            public void HandleStudentUpdate(Guid id, StudentVm student)
            {
                throw new NotImplementedException();
            }
        }
        
        public class ServerRequestor : IServer
        {
            public class RpcRequestMeta
            {
                public int MethodIndex;
            }

            public class RpcPackageMeta
            {
                public enum PackageType : byte
                { 
                    Request,
                    Response
                }

                public PackageType Type;
                /// <remarks>Uses for definition of relations between request and response contexts.</remarks>
                public int Code;
                public int MethodIndex;
            }

            private readonly TcpCommunication _tcpCommunication;

            public ServerRequestor(TcpCommunication tcpCommunication)
            {
                _tcpCommunication = tcpCommunication ?? throw new ArgumentNullException(nameof(tcpCommunication));
            }

            public Guid AddStudent(StudentFm studentFm, int testValue)
            {
                RpcRequestMeta request = new RpcRequestMeta
                {
                    MethodIndex = 0,
                };

                ISerializer<RpcRequestMeta> requestMetaSerializer = DefaultSerializer<RpcRequestMeta>.Instance;
                ISerializer<StudentFm> parameter1Serializer = DefaultSerializer<StudentFm>.Instance;
                Int32Serializer parameter2Serializer = Int32Serializer.Default;

                int bufferLength = requestMetaSerializer.Size(request) +
                                   parameter1Serializer.Size(studentFm) +
                                   parameter2Serializer.Size;

                byte[] buffer = new byte[bufferLength];
                int offset = 0;
                requestMetaSerializer.Serialize(request, buffer, ref offset);
                parameter1Serializer.Serialize(studentFm, buffer, ref offset);
                parameter2Serializer.Serialize(testValue, buffer, ref offset);

                _tcpCommunication.SendWithSize(buffer, 0, buffer.Length);

                return Guid.NewGuid();
            }
            public void DeleteStudent(Guid id)
            {
                RpcRequestMeta request = new RpcRequestMeta
                {
                    MethodIndex = 1,
                };

                ISerializer<RpcRequestMeta> requestMetaSerializer = DefaultSerializer<RpcRequestMeta>.Instance;
                GuidSerializer parameter1Serializer = GuidSerializer.Default;

                int bufferLength = requestMetaSerializer.Size(request) + parameter1Serializer.Size;

                byte[] buffer = new byte[bufferLength];
                int offset = 0;
                requestMetaSerializer.Serialize(request, buffer, ref offset);
                parameter1Serializer.Serialize(id, buffer, ref offset);

                _tcpCommunication.SendWithSize(buffer, 0, buffer.Length);
            }
            public StudentVm GetStudent(Guid id)
            {
                throw new NotImplementedException();
            }
        }


        public class StudentVm
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
        }
        public class StudentFm
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
        public interface IServer
        {
            Guid AddStudent(StudentFm studentFm, int testValue);
            StudentVm GetStudent(Guid id);
            void DeleteStudent(Guid id);
        }
        public interface IClient
        {
            void HandleStudentUpdate(Guid id, StudentVm student);
        }
    }
}
