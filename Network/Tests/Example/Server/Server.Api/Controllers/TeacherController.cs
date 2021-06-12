using Contracts.Server.Controllers;
using Server.Domain.Services.Abstractions;
using System;
using System.Threading.Tasks;

namespace Server.App.Controllers
{
	internal class TeacherController : ITeacherController
	{
		private readonly ITeacherService _teacherService;

		public TeacherController(ITeacherService teacherService)
		{
			_teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService));
		}

		public Task<Guid> AddTeacher(string name)
		{
			return _teacherService.AddTeacher(name);
		}
	}
}
