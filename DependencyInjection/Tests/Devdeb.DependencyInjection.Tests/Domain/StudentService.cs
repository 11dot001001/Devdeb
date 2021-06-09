using Devdeb.DependencyInjection.Tests.Domain.Abstractions;
using Devdeb.DependencyInjection.Tests.Domain.Models;
using System;
using System.Collections.Generic;

namespace Devdeb.DependencyInjection.Tests.Domain
{
	public class StudentService : IStudentService
	{
		static private int _initializaionNumber;
		static private readonly object _initializaionNumberLocker = new object();

		private readonly Dictionary<Guid, Student> _students;
		private readonly IUniversityService _universityService;

		public StudentService(IUniversityService universityService)
		{
			_universityService = universityService ?? throw new ArgumentNullException(nameof(universityService));
			_students = new Dictionary<Guid, Student>();
			lock (_initializaionNumberLocker)
				InitializaionNumber = ++_initializaionNumber;
		}

		public int InitializaionNumber { get; }

		public Guid AddStudent(Student student)
		{
			Guid id = Guid.NewGuid();
			student.Id = id;
			student.UniversityName = _universityService.GetUniversityName;
			_students.Add(id, student);
			return id;
		}

		public IEnumerable<Student> GetStudents() => _students.Values;
	}
}
