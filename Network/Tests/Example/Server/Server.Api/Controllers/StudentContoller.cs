using Contracts.Server.Controllers;
using Models;
using Server.Domain.Services.Abstractions;
using System;
using System.Threading.Tasks;

namespace Server.App.Controllers
{
	internal class StudentContoller : IStudentContoller
	{
		private readonly IStudentService _studentService;

		public StudentContoller(IStudentService studentService)
		{
			_studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
		}

		public int FreeId => _studentService.FreeId;

		public Task<Guid> AddStudent(StudentFm studentFm, int testValue)
		{
			return _studentService.AddStudent(studentFm, testValue);
		}
		public void DeleteStudent(Guid id)
		{
			_studentService.DeleteStudent(id);
		}
		public Task<StudentVm> GetStudent(Guid id)
		{
			return _studentService.GetStudent(id);
		}
	}
}
