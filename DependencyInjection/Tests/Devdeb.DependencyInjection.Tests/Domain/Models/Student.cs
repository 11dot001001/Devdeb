using System;

namespace Devdeb.DependencyInjection.Tests.Domain.Models
{
	public class Student
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public int Age { get; set; }
		public string UniversityName { get; set; }
	}
}
