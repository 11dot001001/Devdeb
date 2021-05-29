using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Interfaces;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Devdeb.Network.Tests.Rpc.BusinessLogic.Domain
{
	public class ServerImplementation : IServer
	{
		private readonly Dictionary<Guid, StudentVm> _students;
		private static int _freeId;
		private static object _freeIdLocker = new object();

		public ServerImplementation()
		{
			_freeId = 0;
			_students = new Dictionary<Guid, StudentVm>();
		}

		public int FreeId
		{
			get
			{
				Console.WriteLine($"Requested free id {_freeId}.");
				lock (_freeIdLocker)
					return _freeId++;
			}
		}

		public async Task<Guid> AddStudent(StudentFm studentFm, int testValue)
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
			await Task.Delay(5000);

			return studentId;
		}
		public void DeleteStudent(Guid id)
		{
			Console.WriteLine($"Student {id} was deleted.");
			_students.Remove(id);
		}
		public Task<StudentVm> GetStudent(Guid id) => Task.FromResult(_students[id]);
	}
}
