using Devdeb.DependencyInjection.Tests.Domain.Models;
using System;
using System.Collections.Generic;

namespace Devdeb.DependencyInjection.Tests.Domain.Abstractions
{
	public interface IStudentService
	{
		Guid AddStudent(Student student);
		IEnumerable<Student> GetStudents();

		int InitializaionNumber { get; }
	}
}
