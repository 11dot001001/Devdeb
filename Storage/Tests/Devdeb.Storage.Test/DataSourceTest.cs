using Devdeb.Serialization;
using Devdeb.Serialization.Serializers;
using Devdeb.Serialization.Serializers.System;
using Devdeb.Sets.Generic;
using Devdeb.Sets.Ratios;
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
			DataSource dataSource = new DataSource(DatabaseDirectoryInfo, MaxHeapSize);
			DataSet<StoredClass> storedClassSet = dataSource.StoredClassSet;

			//StoredClass[] startStoredClasses = storedClassSet.GetAll();

			//StoredClass storedClass1 = new StoredClass() //1004 21.
			//{
			//	Id = 15,
			//	Value = "StoredClass 1"
			//};
			//StoredClass storedClass2 = new StoredClass() //1025 21.
			//{
			//	Id = 435234523,
			//	Value = "StoredClass 1"
			//};
			//StoredClass storedClass3 = new StoredClass() //1046 21.
			//{
			//	Id = 1,
			//	Value = "StoredClass 1"
			//};
			//storedClassSet.Add(storedClass1.Id, storedClass1);
			//storedClassSet.Add(storedClass2.Id, storedClass2);
			//storedClassSet.Add(storedClass3.Id, storedClass3);
			//storedClassSet.RemoveById(15);
			//bool is0WasFound = storedClassSet.TryGetById(0, out StoredClass storedClass0);
			//bool is15WasFound = storedClassSet.TryGetById(15, out StoredClass storedClass15);
			//bool is435234523WasFound = storedClassSet.TryGetById(435234523, out StoredClass storedClass435234523);
			//StoredClass[] storedClasses = storedClassSet.GetAll();

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


			DataSet<Student> studentSet = dataSource.StudentSet;
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
		public class DataSetMeta
		{
			public Segment PrimaryIndexesPointer { get; set; }
		}
		public class Meta
		{
			public Segment StoredClassSetMetaPointer;
			public Segment StudentSetMetaPointer;
		}
		public class StoredReferenceValue<T>
		{
			private Segment _pointer;

			public StoredReferenceValue(Segment pointer, ISerializer<T> serializer) : this(pointer, default, serializer) { }
			public StoredReferenceValue(Segment pointer, T value, ISerializer<T> serializer)
			{
				_pointer = pointer;
				Value = value;
				Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
			}

			public Segment Pointer
			{
				get => _pointer;
				set { _pointer = value; PointerUpdated?.Invoke(this, value); }
			}
			public T Value { get; set; }
			public ISerializer<T> Serializer { get; set; }

			public event EventHandler<Segment> PointerUpdated;
		}
		public class StoredEntity<TEntity>
		{
			public int Version { get; set; }
			public TEntity Data { get; set; }
		}

		public class Migration<TPrevious, TNext>
		{
			public ISerializer<TPrevious> PreviousSerializer { get; set; }
			public ISerializer<TNext> NextSerializer { get; set; }
			public Func<TPrevious, TNext> Convert { get; set; }
		}
		public abstract class EntityMigrator<TCurrentVersion>
		{
			protected EntityMigrator()
			{
				if (CurrentSerializer.Flags == SerializerFlags.NeedCount)
					throw new Exception($"{nameof(CurrentSerializer)} should not need a count parameter in deserialize {nameof(ISerializer<object>.Deserialize)}.");
			}

			public abstract int Version { get; }
			public abstract ISerializer<TCurrentVersion> CurrentSerializer { get; }
			public virtual TCurrentVersion Convert(int version, byte[] buffer, int offset)
			{
				if (version != Version)
					throw new ArgumentException(nameof(version));
				return CurrentSerializer.Deserialize(buffer, offset);
			}
		}
		public abstract class EvolutionEntityMigrator<TCurrentVersion, TPrevious> : EntityMigrator<TCurrentVersion>
		{
			protected EvolutionEntityMigrator()
			{
				if (Version <= PreviousMigrator.Version)
					throw new Exception($"{nameof(Version)} must be greater than previous migrator version.");
			}

			public abstract EntityMigrator<TPrevious> PreviousMigrator { get; }
			protected abstract Migration<TPrevious, TCurrentVersion> Migration { get; }
			public override TCurrentVersion Convert(int version, byte[] buffer, int offset)
			{
				if (version == Version)
					return CurrentSerializer.Deserialize(buffer, offset);
				return Migration.Convert(PreviousMigrator.Convert(version, buffer, offset));
			}
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
			protected override Migration<Student1, Student> Migration => new Migration<Student1, Student>
			{
				PreviousSerializer = Serializers.Student1Serializer,
				NextSerializer = Serializers.StudentSerializer,
				Convert = student1 => new Student
				{
					Id = student1.Id,
					Name = student1.Name,
					Age = student1.Age,
					Degree = null,
				}
			};
		}

		public class DataSource
		{
			private readonly StorableHeap _storableHeap;
			private readonly StoredReferenceValue<Meta> _meta;
			private readonly StoredReferenceValue<DataSetMeta> _storedClassSetMeta;
			private readonly StoredReferenceValue<DataSetMeta> _studentSetMeta;

			public unsafe DataSource(DirectoryInfo heapDirectory, long maxHeapSize)
			{
				_storableHeap = new StorableHeap(heapDirectory, maxHeapSize);
				_meta = InitializeMeta();

				_storedClassSetMeta = InitializeDataSetMeta
				(
					x => _meta.Value.StoredClassSetMetaPointer = x,
					() => _meta.Value.StoredClassSetMetaPointer
				);
				_studentSetMeta = InitializeDataSetMeta
				(
					x => _meta.Value.StudentSetMetaPointer = x,
					() => _meta.Value.StudentSetMetaPointer
				);

				//StoredClassSet = InitializeDataSet(_storedClassSetMeta, Serializers.StoredClassSerializer);
				StudentSet = InitializeDataSet(_studentSetMeta, new StudentMigrator());
			}

			public DataSet<StoredClass> StoredClassSet { get; }
			public DataSet<Student> StudentSet { get; }

			public StoredReferenceValue<T> Initialize<T>(T data, ISerializer<T> serializer)
			{
				int dataSize = serializer.Size(data);
				byte[] buffer = new byte[dataSize];
				serializer.Serialize(data, buffer, 0);
				Segment pointer = _storableHeap.AllocateMemory(dataSize);
				_storableHeap.Write(pointer, buffer, 0, buffer.Length);
				return new StoredReferenceValue<T>(pointer, data, serializer);
			}
			public StoredReferenceValue<T> Load<T>(Segment pointer, ISerializer<T> serializer)
			{
				byte[] buffer = new byte[pointer.Size];
				_storableHeap.ReadBytes(pointer, buffer, 0, buffer.Length);
				T data = serializer.Deserialize(buffer, 0);
				return new StoredReferenceValue<T>(pointer, data, serializer);
			}
			public void Upload<T>(StoredReferenceValue<T> referenceValue)
			{
				int size = referenceValue.Serializer.Size(referenceValue.Value);
				Segment uploadSegment = referenceValue.Pointer;
				bool resized = false;
				if (size != referenceValue.Pointer.Size)
				{
					Debug.Assert(referenceValue.Pointer != default);
					uploadSegment = _storableHeap.AllocateMemory(size);
					resized = true;
				}
				byte[] buffer = new byte[size];
				referenceValue.Serializer.Serialize(referenceValue.Value, buffer, 0);
				_storableHeap.Write(uploadSegment, buffer, 0, buffer.Length);
				if (resized)
					referenceValue.Pointer = uploadSegment;
			}

			private StoredReferenceValue<Meta> InitializeMeta()
			{
				StoredReferenceValue<Meta> meta = null;
				try
				{
					if (_storableHeap.IsInitializationFirst)
					{
						meta = Initialize(new Meta(), Serializers.MetaSeriaizer);
						uploadMetaPointer(meta.Pointer);
						return meta;
					}

					byte[] buffer = new byte[Serializers.SegmentSerializer.Size];
					_storableHeap.ReadBytes(_storableHeap.EntrySegment, buffer, 0, buffer.Length);
					Segment metaPointer = Serializers.SegmentSerializer.Deserialize(buffer, 0);
					meta = Load(metaPointer, Serializers.MetaSeriaizer);
					return meta;
				}
				finally { meta.PointerUpdated += (_, pointer) => uploadMetaPointer(pointer); }

				void uploadMetaPointer(Segment metaPointer)
				{
					byte[] buffer = new byte[Serializers.SegmentSerializer.Size];
					Serializers.SegmentSerializer.Serialize(metaPointer, buffer, 0);
					_storableHeap.Write(_storableHeap.EntrySegment, buffer, 0, buffer.Length);
				}
			}

			private unsafe StoredReferenceValue<DataSetMeta> InitializeDataSetMeta(Action<Segment> updateDataSetMetaPointer, Func<Segment> getDataSetMetaPointer)
			{
				Debug.Assert(_meta != default);
				StoredReferenceValue<DataSetMeta> dataSetMeta = null;
				try
				{
					if (_storableHeap.IsInitializationFirst)
					{
						dataSetMeta = Initialize(new DataSetMeta(), Serializers.DataSetMetaSerializer);
						uploadStoredClassDataSetPointer(dataSetMeta.Pointer);
						return dataSetMeta;
					}
					dataSetMeta = Load(getDataSetMetaPointer(), Serializers.DataSetMetaSerializer);
					return dataSetMeta;
				}
				finally { dataSetMeta.PointerUpdated += (_, pointer) => uploadStoredClassDataSetPointer(pointer); }

				void uploadStoredClassDataSetPointer(Segment pointer)
				{
					updateDataSetMetaPointer(pointer);
					Upload(_meta);
				}
			}
			private DataSet<T> InitializeDataSet<T>(StoredReferenceValue<DataSetMeta> dataSetMeta, EntityMigrator<T> entityMigrator)
			{
				Debug.Assert(dataSetMeta != default);
				StoredReferenceValue<RedBlackTreeSurjection<int, Segment>> primaryIndexes = null;

				if (_storableHeap.IsInitializationFirst)
				{
					primaryIndexes = Initialize(new RedBlackTreeSurjection<int, Segment>(), Serializers.IndexesSerializer);
					uploadPrimaryIndexesPointer(primaryIndexes.Pointer);
				}
				else
					primaryIndexes = Load(dataSetMeta.Value.PrimaryIndexesPointer, Serializers.IndexesSerializer);

				primaryIndexes.PointerUpdated += (_, pointer) => uploadPrimaryIndexesPointer(pointer);
				return new DataSet<T>(_storableHeap, this, primaryIndexes, entityMigrator);

				void uploadPrimaryIndexesPointer(Segment pointer)
				{
					dataSetMeta.Value.PrimaryIndexesPointer = pointer;
					Upload(dataSetMeta);
				}
			}
		}
		public class DataSet<T>
		{
			private readonly StorableHeap _storableHeap;
			private readonly DataSource _dataSource;
			private readonly StoredReferenceValue<RedBlackTreeSurjection<int, Segment>> _primaryIndexes;
			EntityMigrator<T> _entityMigrator;
			private readonly StoredEntitySerializer<T> _storedEntitySerializer;

			public DataSet
			(
				StorableHeap storableHeap,
				DataSource dataSource,
				StoredReferenceValue<RedBlackTreeSurjection<int, Segment>> primaryIndexes,
				EntityMigrator<T> entityMigrator
			)
			{
				_storableHeap = storableHeap ?? throw new ArgumentNullException(nameof(storableHeap));
				_dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
				_primaryIndexes = primaryIndexes ?? throw new ArgumentNullException(nameof(primaryIndexes));
				_entityMigrator = entityMigrator ?? throw new ArgumentNullException(nameof(entityMigrator));
				_storedEntitySerializer = new StoredEntitySerializer<T>(_entityMigrator);
			}

			public bool Add(int id, T instance)
			{
				if (_primaryIndexes.Value.TryGetValue(id, out _))
					return false;

				StoredEntity<T> storedEntity = new StoredEntity<T>
				{
					Version = _entityMigrator.Version,
					Data = instance
				};

				int instanceLength = _storedEntitySerializer.Size(storedEntity);
				Segment instanceSegment = _storableHeap.AllocateMemory(instanceLength);
				_primaryIndexes.Value.Add(id, instanceSegment);

				byte[] buffer = new byte[instanceLength];
				_storedEntitySerializer.Serialize(storedEntity, buffer, 0);
				_storableHeap.Write(instanceSegment, buffer, 0, buffer.Length);
				_dataSource.Upload(_primaryIndexes);
				return true;
			}
			public bool TryGetById(int id, out T instance)
			{
				if (!_primaryIndexes.Value.TryGetValue(id, out Segment segment))
				{
					instance = default;
					return false;
				}

				byte[] buffer = new byte[segment.Size]; //segment.Size incredible crutch may be.
				_storableHeap.ReadBytes(segment, buffer, 0, buffer.Length);
				instance = _storedEntitySerializer.Deserialize(buffer, 0).Data;
				return true;
			}
			public void RemoveById(int id)
			{
				if (!_primaryIndexes.Value.Remove(id, out Segment segment))
					return;
				_storableHeap.FreeMemory(segment);
				_dataSource.Upload(_primaryIndexes);
			}
			public T[] GetAll()
			{
				Segment[] segments = _primaryIndexes.Value.Select(x => x.Output).ToArray();
				T[] result = new T[segments.Length];
				for (int i = 0; i != result.Length; i++)
				{
					byte[] buffer = new byte[segments[i].Size]; //segment.Size incredible crutch may be.
					_storableHeap.ReadBytes(segments[i], buffer, 0, buffer.Length);
					result[i] = _storedEntitySerializer.Deserialize(buffer, 0).Data;
				}
				return result;
			}
		}

		static internal class Serializers
		{
			static Serializers()
			{
				Int32Serializer = new Int32Serializer();
				SegmentSerializer = new SegmentSerializer();
				IndexesSerializer = new IndexesSerializer();
				StoredClassSerializer = new StoredClassSerializer();
				Student1Serializer = new Student1Serializer();
				StudentSerializer = new StudentSerializer();
				DataSetMetaSerializer = new DataSetMetaSerializer();
				MetaSeriaizer = new MetaSeriaizer();
			}

			static public Int32Serializer Int32Serializer { get; }
			static public SegmentSerializer SegmentSerializer { get; }
			static public IndexesSerializer IndexesSerializer { get; }
			static public StoredClassSerializer StoredClassSerializer { get; }
			static public Student1Serializer Student1Serializer { get; }
			static public StudentSerializer StudentSerializer { get; }
			static public DataSetMetaSerializer DataSetMetaSerializer { get; }
			static public MetaSeriaizer MetaSeriaizer { get; }
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
		public class DataSetMetaSerializer : ConstantLengthSerializer<DataSetMeta>
		{
			public DataSetMetaSerializer() : base(Serializers.SegmentSerializer.Size) { }

			public override void Serialize(DataSetMeta instance, byte[] buffer, int offset)
			{
				VerifySerialize(instance, buffer, offset);
				Serializers.SegmentSerializer.Serialize(instance.PrimaryIndexesPointer, buffer, offset);
			}
			public override DataSetMeta Deserialize(byte[] buffer, int offset)
			{
				VerifyDeserialize(buffer, offset);
				Segment indexesPointer = Serializers.SegmentSerializer.Deserialize(buffer, offset);
				return new DataSetMeta() { PrimaryIndexesPointer = indexesPointer };
			}
		}
		public class MetaSeriaizer : ConstantLengthSerializer<Meta>
		{
			public MetaSeriaizer() : base(Serializers.DataSetMetaSerializer.Size * 2) { }

			public override void Serialize(Meta instance, byte[] buffer, int offset)
			{
				VerifySerialize(instance, buffer, offset);
				Serializers.SegmentSerializer.Serialize(instance.StoredClassSetMetaPointer, buffer, ref offset);
				Serializers.SegmentSerializer.Serialize(instance.StudentSetMetaPointer, buffer, offset);
			}
			public override Meta Deserialize(byte[] buffer, int offset)
			{
				VerifyDeserialize(buffer, offset);
				Segment storedClassSetMeta = Serializers.SegmentSerializer.Deserialize(buffer, ref offset);
				Segment studentSetMeta = Serializers.SegmentSerializer.Deserialize(buffer, offset);
				return new Meta
				{
					StoredClassSetMetaPointer = storedClassSetMeta,
					StudentSetMetaPointer = studentSetMeta
				};
			}
		}
		public class IndexesSerializer : Serializer<RedBlackTreeSurjection<int, Segment>>
		{
			private readonly ArrayLengthSerializer<SurjectionRatio<int, Segment>> _arraySerializer;

			public IndexesSerializer()
			{
				SurjectionRatioSerializer<int, Segment> indexSerializer = new SurjectionRatioSerializer<int, Segment>
				(
					new Int32Serializer(),
					Serializers.SegmentSerializer
				);
				_arraySerializer = new ArrayLengthSerializer<SurjectionRatio<int, Segment>>
				(
					indexSerializer
				);
			}

			public override int Size(RedBlackTreeSurjection<int, Segment> instance)
			{
				return _arraySerializer.Size(instance.ToArray());
			}
			public override void Serialize(RedBlackTreeSurjection<int, Segment> instance, byte[] buffer, int offset)
			{
				_arraySerializer.Serialize(instance.ToArray(), buffer, offset);
			}
			public override RedBlackTreeSurjection<int, Segment> Deserialize(byte[] buffer, int offset, int? count = null)
			{
				return new RedBlackTreeSurjection<int, Segment>(_arraySerializer.Deserialize(buffer, offset));
			}
		}
		public class StoredEntitySerializer<TEntity> : Serializer<StoredEntity<TEntity>>
		{
			private readonly EntityMigrator<TEntity> _entityMigrator;
			private readonly Int32Serializer _int32Serializer;

			public StoredEntitySerializer(EntityMigrator<TEntity> entityMigrator)
			{
				_entityMigrator = entityMigrator ?? throw new ArgumentNullException(nameof(entityMigrator));
				_int32Serializer = new Int32Serializer();
			}

			public override int Size(StoredEntity<TEntity> instance)
			{
				VerifySize(instance);
				return _int32Serializer.Size + _entityMigrator.CurrentSerializer.Size(instance.Data);
			}
			public override void Serialize(StoredEntity<TEntity> instance, byte[] buffer, int offset)
			{
				VerifySerialize(instance, buffer, offset);
				_int32Serializer.Serialize(instance.Version, buffer, ref offset);
				_entityMigrator.CurrentSerializer.Serialize(instance.Data, buffer, ref offset);
			}
			public override StoredEntity<TEntity> Deserialize(byte[] buffer, int offset, int? count = null)
			{
				VerifyDeserialize(buffer, offset, count);
				int version = _int32Serializer.Deserialize(buffer, ref offset);
				TEntity entity = _entityMigrator.Convert(version, buffer, offset);
				return new StoredEntity<TEntity>
				{
					Version = version,
					Data = entity
				};
			}
		}
	}
}
