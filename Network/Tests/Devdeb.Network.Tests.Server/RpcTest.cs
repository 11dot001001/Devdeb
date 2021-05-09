using Devdeb.Network.TCP.Communication;
using Devdeb.Network.TCP.Expecting;
using Devdeb.Serialization;
using Devdeb.Serialization.Default;
using Devdeb.Serialization.Serializers;
using Devdeb.Serialization.Serializers.System;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Reflection;

namespace Devdeb.Network.Tests.Server
{
    public class RpcTest
    {
        static private readonly IPAddress _iPAddress = IPAddress.Parse("127.0.0.1");
        static private readonly int _port = 25000;
        static private readonly int _backlog = 1;

        public void Test()
        {
            RpcServer rpcServer = new RpcServer(_iPAddress, _port, _backlog);
            rpcServer.Start();
            Console.ReadKey();
        }

        public abstract class BaseRpcServer<TServer, TClient> : BaseExpectingTcpServer 
        {
            public BaseRpcServer(IPAddress iPAddress, int port, int backlog) : base(iPAddress, port, backlog) 
            {
                Type serverInterfaceType = typeof(TServer);
                Type rpcServerType = this.GetType();

                MethodInfo[] interfaceMethods = serverInterfaceType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            }
        }

        public class RpcServer : BaseRpcServer<IServer, IClient>, IServer
        {
            public class RpcRequestMeta
            {
                public int MethodIndex;
            }

            private readonly Dictionary<Guid, StudentVm> _students;

            public RpcServer(IPAddress iPAddress, int port, int backlog) : base(iPAddress, port, backlog) 
            {
                _students = new Dictionary<Guid, StudentVm>();
            }

            protected override void ProcessCommunication(TcpCommunication tcpCommunication, int count)
            {
                byte[] buffer = new byte[count];
                tcpCommunication.Receive(buffer, 0, count);

                int offset = 0;
                RpcRequestMeta requestMeta = DefaultSerializer<RpcRequestMeta>.Instance.Deserialize(buffer, ref offset);

                if(requestMeta.MethodIndex == 0)
                {
                    ISerializer<StudentFm> parameter1Serializer = DefaultSerializer<StudentFm>.Instance;
                    Int32Serializer parameter2Serializer = Int32Serializer.Default;

                    StudentFm parameter1 = parameter1Serializer.Deserialize(buffer, ref offset);
                    int parameter2 = parameter2Serializer.Deserialize(buffer, ref offset);
                    AddStudent(parameter1, parameter2);
                }
                else if (requestMeta.MethodIndex == 1)
                {
                    GuidSerializer parameter1Serializer = GuidSerializer.Default;

                    Guid parameter1 = parameter1Serializer.Deserialize(buffer, ref offset);
                    DeleteStudent(parameter1);
                }
            }

            public Guid AddStudent(StudentFm studentFm, int testValue)
            {
                Guid studentId = Guid.NewGuid();
                _students.Add(
                    studentId,
                    new StudentVm
                    {
                        Id = studentId,
                        Name = studentFm.Name,
                        Age = studentFm.Age
                    }
                );
                Console.WriteLine($"Student {studentFm.Name} was added with id {studentId}. TestValue {testValue}.");
                return studentId;
            }
            public void DeleteStudent(Guid id)
            {
                Console.WriteLine($"Student {id} was deleted.");
                _students.Remove(id);
            }

            public StudentVm GetStudent(Guid id) => _students.GetValueOrDefault(id);

            public void HandleStudentUpdate(Guid id, StudentVm student)
            {

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
