using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Abstractions.Server;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Server
{
	public class ServerStudentContoller : IStudentContoller
	{
		private readonly Dictionary<Guid, StudentVm> _students;
		private static int _freeId;
		private static readonly object _freeIdLocker = new object();
		
		public ServerStudentContoller()
		{
			_freeId = 0;
			_students = new Dictionary<Guid, StudentVm>();
		}

		public int FreeId
		{
			get
			{
				lock (_freeIdLocker)
				{
					Console.WriteLine($"Returned id {_freeId}.");
					return _freeId++;
				}
			}
		}

		public Task<Guid> AddStudent(StudentFm studentFm, int testValue)
		{
			Guid studentId = Guid.NewGuid();

			lock (_students)
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

			return Task.FromResult(studentId);
		}
		public void DeleteStudent(Guid id)
		{
			Console.WriteLine($"Student {id} was deleted.");
			_students.Remove(id);
		}
		public Task<StudentVm> GetStudent(Guid id) => Task.FromResult(_students[id]);
	}
}
