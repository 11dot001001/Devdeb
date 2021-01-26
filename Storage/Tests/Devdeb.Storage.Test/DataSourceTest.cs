using Devdeb.Serialization;
using Devdeb.Serialization.Serializers;
using Devdeb.Serialization.Serializers.System;
using Devdeb.Sets.Generic;
using Devdeb.Sets.Ratios;
using Devdeb.Storage.Migrators;
using Devdeb.Storage.Serializers;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

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
		}

		public class DataContext : DataSource
		{
			public DataContext(DirectoryInfo heapDirectory, long maxHeapSize) : base(heapDirectory, maxHeapSize) 
			{
				Students = InitializeDataSet(1, new StudentMigrator());
				StoredClasses = InitializeDataSet(2, new StoredClassMigrator());
			}

			public DataSet<Student> Students { get; }
			public DataSet<StoredClass> StoredClasses { get; }
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
				Int32Serializer = new Int32Serializer();
				StoredClassSerializer = new StoredClassSerializer();
				Student1Serializer = new Student1Serializer();
				StudentSerializer = new StudentSerializer();
			}

			static public Int32Serializer Int32Serializer { get; }
			static public StoredClassSerializer StoredClassSerializer { get; }
			static public Student1Serializer Student1Serializer { get; }
			static public StudentSerializer StudentSerializer { get; }
		}
		public class Student1Serializer : Serializer<Student1>
		{
			private readonly Int32Serializer _int32Serializer;
			private readonly StringLengthSerializer _stringLengthSerializer;

			public Student1Serializer()
			{
				_int32Serializer = new Int32Serializer();
				_stringLengthSerializer = new StringLengthSerializer(Encoding.Default);
			}

			public override int Size(Student1 instance)
			{
				return _int32Serializer.Size * 2 + _stringLengthSerializer.Size(instance.Name);
			}
			public override void Serialize(Student1 instance, byte[] buffer, int offset)
			{
				_int32Serializer.Serialize(instance.Id, buffer, ref offset);
				_stringLengthSerializer.Serialize(instance.Name, buffer, ref offset);
				_int32Serializer.Serialize(instance.Age, buffer, offset);
			}
			public override Student1 Deserialize(byte[] buffer, int offset, int? count = null)
			{
				int id = _int32Serializer.Deserialize(buffer, ref offset);
				string name = _stringLengthSerializer.Deserialize(buffer, ref offset);
				int age = _int32Serializer.Deserialize(buffer, offset);
				return new Student1
				{
					Id = id,
					Name = name,
					Age = age
				};
			}
		}
		public class StudentSerializer : Serializer<Student>
		{
			private readonly Int32Serializer _int32Serializer;
			private readonly StringLengthSerializer _stringLengthSerializer;

			public StudentSerializer()
			{
				_int32Serializer = new Int32Serializer();
				_stringLengthSerializer = new StringLengthSerializer(Encoding.Default);
			}

			public override int Size(Student instance)
			{
				return _int32Serializer.Size * 2 +
					   _stringLengthSerializer.Size(instance.Name) +
					  _stringLengthSerializer.Size(instance.Degree);
			}
			public override void Serialize(Student instance, byte[] buffer, int offset)
			{
				_int32Serializer.Serialize(instance.Id, buffer, ref offset);
				_stringLengthSerializer.Serialize(instance.Name, buffer, ref offset);
				_int32Serializer.Serialize(instance.Age, buffer, ref offset);
				_stringLengthSerializer.Serialize(instance.Degree, buffer, offset);
			}
			public override Student Deserialize(byte[] buffer, int offset, int? count = null)
			{
				int id = _int32Serializer.Deserialize(buffer, ref offset);
				string name = _stringLengthSerializer.Deserialize(buffer, ref offset);
				int age = _int32Serializer.Deserialize(buffer, ref offset);
				string degree = _stringLengthSerializer.Deserialize(buffer, ref offset);
				return new Student
				{
					Id = id,
					Name = name,
					Age = age,
					Degree = degree
				};
			}
		}
		public class StoredClassSerializer : Serializer<StoredClass>
		{
			private readonly Int32Serializer _int32Serializer;
			private readonly StringLengthSerializer _stringLengthSerializer;

			public StoredClassSerializer()
			{
				_int32Serializer = new Int32Serializer();
				_stringLengthSerializer = new StringLengthSerializer(Encoding.Default);
			}

			public override int Size(StoredClass instance)
			{
				VerifySize(instance);
				return _int32Serializer.Size + _stringLengthSerializer.Size(instance.Value);
			}
			public override void Serialize(StoredClass instance, byte[] buffer, int offset)
			{
				VerifySerialize(instance, buffer, offset);
				_int32Serializer.Serialize(instance.Id, buffer, ref offset);
				_stringLengthSerializer.Serialize(instance.Value, buffer, offset);
			}
			public override StoredClass Deserialize(byte[] buffer, int offset, int? count = null)
			{
				VerifyDeserialize(buffer, offset, count);
				int id = _int32Serializer.Deserialize(buffer, ref offset);
				string value = _stringLengthSerializer.Deserialize(buffer, offset);
				return new StoredClass
				{
					Id = id,
					Value = value
				};
			}
		}
	}
}
