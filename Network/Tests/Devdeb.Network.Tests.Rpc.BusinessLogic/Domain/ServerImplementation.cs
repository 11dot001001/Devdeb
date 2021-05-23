using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Interfaces;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Models;
using System;
using System.Collections.Generic;

namespace Devdeb.Network.Tests.Rpc.BusinessLogic.Domain
{
	public class ServerImplementation : IServer
	{
		private readonly Dictionary<Guid, StudentVm> _students;
		private static int _freeId;

		public ServerImplementation()
		{
			_freeId = 0;
			_students = new Dictionary<Guid, StudentVm>();
		}

		public int FreeId => _freeId++;

		public void AddStudent(StudentFm studentFm, int testValue)
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
		}
		public void DeleteStudent(Guid id)
		{
			Console.WriteLine($"Student {id} was deleted.");
			_students.Remove(id);
		}
		public StudentVm GetStudent(Guid id) => _students.GetValueOrDefault(id);
	}
}
