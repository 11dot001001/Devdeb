using Devdeb.Serialization;
using Devdeb.Serialization.Builders;
using Devdeb.Storage.Migrators;
using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

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

			DataSet<Student> studentSet = dataContext.Students;
            var a = studentSet.GetAll();
            studentSet.TryGetById(2, out Student firstStudent);
            Student relatedStudent = null;
            if (firstStudent.RelatedStudentId.HasValue)
                studentSet.TryGetById(firstStudent.RelatedStudentId.Value, out relatedStudent);

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
				Degree = "bachelor",
				RelatedStudentId = student1.Id
			};
			studentSet.Add(student1.Id, student1);
			studentSet.Add(student2.Id, student2);
			Student[] endStudents = studentSet.GetAll();

			EntityTypeBuilder<Student> studentBuilder = new EntityTypeBuilder<Student>();
			studentBuilder.SetPrimaryKey(x => x.Id);
			studentBuilder.AddIndex(x => x.Name);
			studentBuilder.AddForeignKey(x => x.RelatedStudent, x => x.RelatedStudentId);
		}

		public class EntityTypeBuilder<T>
		{
			private MemberInfo _primaryKey;

			public void SetPrimaryKey(Expression<Func<T, object>> selection)
			{
				
			}
			public void AddIndex(Expression<Func<T, object>> selection)
			{

			}
			public void AddForeignKey
			(
				Expression<Func<T, object>> selection,
				Expression<Func<T, object>> navigationProperty
			)
			{

			}

			public void Build()
			{
				if (_primaryKey == null)
					throw new Exception("");
			}
		}

		public class DataContext : DataSource
		{
			public DataContext(DirectoryInfo heapDirectory, long maxHeapSize) : base(heapDirectory, maxHeapSize)
			{
				Students = InitializeDataSet(1, new StudentMigrator());
			}

			public DataSet<Student> Students { get; }
		}

		public class Student
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public int Age { get; set; }
			public string Degree { get; set; }

			public int? RelatedStudentId { get; set; }
			public Student RelatedStudent { get; set; }
		}

		public sealed class StudentMigrator : EntityMigrator<Student>
		{
			static private readonly ISerializer<Student> _serializer;

			static StudentMigrator()
			{
				SerializerBuilder<Student> serializerBuilder = new SerializerBuilder<Student>();
				_ = serializerBuilder.AddMember(x => x.Id);
				_ = serializerBuilder.AddMember(x => x.Name);
				_ = serializerBuilder.AddMember(x => x.Age);
				_ = serializerBuilder.AddMember(x => x.Degree);
				_ = serializerBuilder.AddMember(x => x.RelatedStudentId);
				_serializer = serializerBuilder.Build();
			}

			public override ISerializer<Student> CurrentSerializer => _serializer;
			public override int Version => 1;
		}
	}
}
