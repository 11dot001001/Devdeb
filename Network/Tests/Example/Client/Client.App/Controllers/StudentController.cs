using Client.Domain.Services.Abstractions;
using Contracts.Client.Controllers;
using Models;
using System;

namespace Client.App.Controllers
{
	internal class StudentController : IStudentController
	{
		private readonly IStudentService _studentService;

		public StudentController(IStudentService studentService)
		{
			_studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
		}

		public void HandleStudentUpdate(Guid id, StudentVm student)
		{
			_studentService.HandleStudentUpdate(id, student);
		}
	}
}
