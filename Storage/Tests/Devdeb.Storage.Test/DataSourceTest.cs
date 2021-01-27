using Devdeb.Serialization;
using Devdeb.Serialization.Serializers;
using Devdeb.Serialization.Serializers.System;
using Devdeb.Storage.Migrators;
using System.IO;

namespace Devdeb.Storage.Test.DataSourceTests
{
	internal class DataSourceTest
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

			bool isMainSudentConatins = dataContext.MainSudent.Contains();
			bool a = dataContext.MainSudent.Add(student1);
			var get1 = dataContext.MainSudent.Get();
			dataContext.MainSudent.Remove();
			var get2 = dataContext.MainSudent.Get();
			bool isMainSudentConatins2 = dataContext.MainSudent.Contains();
		}

		public class DataContext : DataSource
		{
			public DataContext(DirectoryInfo heapDirectory, long maxHeapSize) : base(heapDirectory, maxHeapSize)
			{
				Students = InitializeDataSet(1, new StudentMigrator());
				StoredClasses = InitializeDataSet(2, new StoredClassMigrator());
				MainSudent = InitializeData(1, new StudentMigrator());
			}

			public DataSet<Student> Students { get; }
			public DataSet<StoredClass> StoredClasses { get; }
			public Data<Student> MainSudent { get; }
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
		}

		public sealed class Student1Migrator : EntityMigrator<Student1>
		{
			public override ISerializer<Student1> CurrentSerializer => Serializers.Student1Serializer;
			public override int Version => 0;
		}
		public sealed class StudentMigrator : EvolutionEntityMigrator<Student, Student1>
		{
			public override ISerializer<Student> CurrentSerializer => Serializers.StudentSerializer;
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
			public override ISerializer<StoredClass> CurrentSerializer => Serializers.StoredClassSerializer;
			public override int Version => 0;
		}

		static internal class Serializers
		{
			static Serializers()
			{
				StoredClassSerializer = new StoredClassSerializer();
				Student1Serializer = new Student1Serializer();
				StudentSerializer = new StudentSerializer();
			}

			static public StoredClassSerializer StoredClassSerializer { get; }
			static public Student1Serializer Student1Serializer { get; }
			static public StudentSerializer StudentSerializer { get; }
		}
		public class Student1Serializer : Serializer<Student1>
		{
			public override int Size(Student1 instance)
			{
				VerifySize(instance);
				return Int32Serializer.Default.Size * 2 + StringLengthSerializer.Default.Size(instance.Name);
			}
			public override void Serialize(Student1 instance, byte[] buffer, int offset)
			{
				VerifySerialize(instance, buffer, offset);
				Int32Serializer.Default.Serialize(instance.Id, buffer, ref offset);
				StringLengthSerializer.Default.Serialize(instance.Name, buffer, ref offset);
				Int32Serializer.Default.Serialize(instance.Age, buffer, offset);
			}
			public override Student1 Deserialize(byte[] buffer, int offset, int? count = null)
			{
				VerifyDeserialize(buffer, offset, count);
				return new Student1
				{
					Id = Int32Serializer.Default.Deserialize(buffer, ref offset),
					Name = StringLengthSerializer.Default.Deserialize(buffer, ref offset),
					Age = Int32Serializer.Default.Deserialize(buffer, offset)
				};
			}
		}
		public class StudentSerializer : Serializer<Student>
		{
			public override int Size(Student instance)
			{
				VerifySize(instance);
				return Int32Serializer.Default.Size * 2 +
					   StringLengthSerializer.Default.Size(instance.Name) +
					  StringLengthSerializer.Default.Size(instance.Degree);
			}
			public override void Serialize(Student instance, byte[] buffer, int offset)
			{
				VerifySerialize(instance, buffer, offset);
				Int32Serializer.Default.Serialize(instance.Id, buffer, ref offset);
				StringLengthSerializer.Default.Serialize(instance.Name, buffer, ref offset);
				Int32Serializer.Default.Serialize(instance.Age, buffer, ref offset);
				StringLengthSerializer.Default.Serialize(instance.Degree, buffer, offset);
			}
			public override Student Deserialize(byte[] buffer, int offset, int? count = null)
			{
				VerifyDeserialize(buffer, offset, count);
				return new Student
				{
					Id = Int32Serializer.Default.Deserialize(buffer, ref offset),
					Name = StringLengthSerializer.Default.Deserialize(buffer, ref offset),
					Age = Int32Serializer.Default.Deserialize(buffer, ref offset),
					Degree = StringLengthSerializer.Default.Deserialize(buffer, ref offset)
				};
			}
		}
		public class StoredClassSerializer : Serializer<StoredClass>
		{
			public override int Size(StoredClass instance)
			{
				VerifySize(instance);
				return Int32Serializer.Default.Size + StringLengthSerializer.Default.Size(instance.Value);
			}
			public override void Serialize(StoredClass instance, byte[] buffer, int offset)
			{
				VerifySerialize(instance, buffer, offset);
				Int32Serializer.Default.Serialize(instance.Id, buffer, ref offset);
				StringLengthSerializer.Default.Serialize(instance.Value, buffer, offset);
			}
			public override StoredClass Deserialize(byte[] buffer, int offset, int? count = null)
			{
				VerifyDeserialize(buffer, offset, count);
				return new StoredClass
				{
					Id = Int32Serializer.Default.Deserialize(buffer, ref offset),
					Value = StringLengthSerializer.Default.Deserialize(buffer, offset)
				};
			}
		}
	}
}
