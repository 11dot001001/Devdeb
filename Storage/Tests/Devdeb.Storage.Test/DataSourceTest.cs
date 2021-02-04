using Devdeb.Serialization;
using Devdeb.Serialization.Builders;
using Devdeb.Serialization.Default;
using Devdeb.Serialization.Serializers;
using Devdeb.Serialization.Serializers.System;
using Devdeb.Storage.Migrators;
using System.IO;

namespace Devdeb.Storage.Test.DataSourceTests
{
	public class DataSourceTest
	{
		public const string DatabaseDirectory = @"C:\Users\lehac\Desktop\data";
		public const long MaxHeapSize = 10000;
		public DirectoryInfo DatabaseDirectoryInfo => new DirectoryInfo(DatabaseDirectory);

		public void Test()
		{
			DataContext dataContext = new DataContext(DatabaseDirectoryInfo, MaxHeapSize);

			DataSet<StoredClass> storedClassesSet = dataContext.StoredClasses;
			StoredClass[] startStoredClasses = storedClassesSet.GetAll();

			StoredClass storedClass1 = new StoredClass() //1004 21.
			{
				Id = 15,
				Value = "StoredClass 1"
			};
			StoredClass storedClass2 = new StoredClass() //1025 21.
			{
				Id = 435234523,
				Value = "StoredClass 1"
			};
			StoredClass storedClass3 = new StoredClass() //1046 21.
			{
				Id = 1,
				Value = "StoredClass 1"
			};
			storedClassesSet.Add(storedClass1.Id, storedClass1);
			storedClassesSet.Add(storedClass2.Id, storedClass2);
			storedClassesSet.Add(storedClass3.Id, storedClass3);
			storedClassesSet.RemoveById(15);
			bool is0WasFound = storedClassesSet.TryGetById(0, out StoredClass storedClass0);
			bool is15WasFound = storedClassesSet.TryGetById(15, out StoredClass storedClass15);
			bool is435234523WasFound = storedClassesSet.TryGetById(435234523, out StoredClass storedClass435234523);
			StoredClass[] storedClasses = storedClassesSet.GetAll();

			//DataSet<Student1> oldStudentSet = dataSource.StudentSet;
			//Student1[] oldStartStudents = oldStudentSet.GetAll();
			//Student1 oldVersion = new Student1
			//{
			//	Id = 0,
			//	Name = "Obsolete Version",
			//	Age = 10,
			//};
			//oldStudentSet.Add(oldVersion.Id, oldVersion);
			//Student1[] oldEndStudents = oldStudentSet.GetAll();


			DataSet<Student> studentSet = dataContext.Students;
			Student[] startStudents = studentSet.GetAll();
			Student student1 = new Student
			{
				Id = 1,
				Name = "Valer Volodia",
				Age = 20,
				Degree = "bachelor"
			};
			Student student2 = new Student
			{
				Id = 2,
				Name = "Lev Belov",
				Age = 22,
				Degree = "bachelor"
			};
			studentSet.Add(student1.Id, student1);
			studentSet.Add(student2.Id, student2);
			Student[] endStudents = studentSet.GetAll();

			bool isMainSudentConatins = dataContext.MainStudent.Contains();
			bool a = dataContext.MainStudent.Add(student1);
			var get1 = dataContext.MainStudent.Get();
			dataContext.MainStudent.Remove();
			var get2 = dataContext.MainStudent.Get();
			bool isMainSudentConatins2 = dataContext.MainStudent.Contains();
		}

		public class DataContext : DataSource
		{
			public DataContext(DirectoryInfo heapDirectory, long maxHeapSize) : base(heapDirectory, maxHeapSize)
			{
				Students = InitializeDataSet(1, new StudentMigrator());
				StoredClasses = InitializeDataSet(2, new StoredClassMigrator());
				MainStudent = InitializeData(1, new StudentMigrator());
			}

			public DataSet<Student> Students { get; }
			public DataSet<StoredClass> StoredClasses { get; }
			public Data<Student> MainStudent { get; }
		}

		public class StoredClass
		{
			public int Id { get; set; }
			public string Value { get; set; }
		}
		public class Student1
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public int Age { get; set; }
		}
		public class Student
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public int Age { get; set; }
			public string Degree { get; set; }

			public int RelatedStudentId { get; set; }
			public Student RelatedStudent { get; set; }
		}

		public sealed class Student1Migrator : EntityMigrator<Student1>
		{
			public override ISerializer<Student1> CurrentSerializer => DefaultSerializer<Student1>.Instance;
			public override int Version => 0;
		}
		public sealed class StudentMigrator : EvolutionEntityMigrator<Student, Student1>
		{
			static private readonly ISerializer<Student> _serializer;

			static StudentMigrator()
			{
				SerializerBuilder<Student> serializerBuilder = new SerializerBuilder<Student>();
				_ = serializerBuilder.AddMember(x => x.Id);
				_ = serializerBuilder.AddMember(x => x.Name);
				_ = serializerBuilder.AddMember(x => x.Age);
				_ = serializerBuilder.AddMember(x => x.Degree);
				_serializer = serializerBuilder.Build();
			}

			public override ISerializer<Student> CurrentSerializer => _serializer;
			public override EntityMigrator<Student1> PreviousMigrator => new Student1Migrator();
			public override int Version => 1;
			public override Student Convert(Student1 previous) => new Student
			{
				Id = previous.Id,
				Name = previous.Name,
				Age = previous.Age,
				Degree = null,
			};
		}
		public sealed class StoredClassMigrator : EntityMigrator<StoredClass>
		{
			public override ISerializer<StoredClass> CurrentSerializer => DefaultSerializer<StoredClass>.Instance;
			public override int Version => 0;
		}
	}
}
