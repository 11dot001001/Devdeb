using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Devdeb.Serialization
{
    [TestClass]
    public class SerializationTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            IntegerSerializer1 integerSerializer = new IntegerSerializer1();
            int a = -2;
            byte[] b = integerSerializer.Serialize(a);
            int z = integerSerializer.Deserialize(b);

            Class instance = new Class() { Field = 323, IntProperty = 42443234 };
            ClassSerializer1 classSerializer = new ClassSerializer1();
            byte[] bytes = classSerializer.Serialize(instance);
            Class instance2 = classSerializer.Deserialize(bytes);
        }
        [TestMethod]
        public void TestMethod2()
        {
            string str = "helloHELLOприветПРИВЕТ123"; // 25 length - 12 russian - 10 english - 3 numbers
            int integer = 121212233;
            StringSerializer2 stringSerializer = new StringSerializer2();
            IntegerSerializer2 integerSerializer2 = new IntegerSerializer2();
            IEnumerableSerializer2<string> enumerableSerializer2 = new IEnumerableSerializer2<string>(stringSerializer);
            List<string> strings = new List<string>() { str, "NewString", "NewString228" };

            byte[] buffer = new byte[1024];
            int writeOffset = 0;
            stringSerializer.Serialize(str, buffer, ref writeOffset);
            enumerableSerializer2.Serialize(strings, buffer, ref writeOffset);
            integerSerializer2.Serialize(integer, buffer, ref writeOffset);

            int readOffset = 0;
            string str2 = stringSerializer.Deserialize(buffer, ref readOffset);
            List<string> newList = enumerableSerializer2.Deserialize(buffer, ref readOffset).ToList();
            int integer2 = integerSerializer2.Deserialize(buffer, ref readOffset);
        }
        [TestMethod]
        public void TestMethod3()
        {
            string str = "helloHELLOприветПРИВЕТ123";
            StringSerializer3 stringSerializer = new StringSerializer3();
            IEnumerableSerializer3<string> enumerableSerializer3 = new IEnumerableSerializer3<string>(stringSerializer);
            List<string> strings = new List<string>() { str, "NewString", "NewString228" };

            byte[] buffer = new byte[stringSerializer.Count(str) + enumerableSerializer3.Count(strings)];
            int writeOffset = 0;
            enumerableSerializer3.Serialize(strings, buffer, ref writeOffset);
            stringSerializer.Serialize(str, buffer, ref writeOffset);

            int readOffset = 0;
            List<string> newList = enumerableSerializer3.Deserialize(buffer, ref readOffset).ToList();
            string str2 = stringSerializer.Deserialize(buffer, ref readOffset);
        }
        [TestMethod]
        public void TestMethod4()
        {
            Class instance = new Class()
            {
                Field = 32323,
                Strings = new List<string>() { "NewString", "NewString228" },
                IntProperty = 2281488,
                StringProperty = "Гы"
            };
            IntegerSerializer3 integerSerializer = new IntegerSerializer3();
            StringSerializer3 stringSerializer = new StringSerializer3();
            IEnumerableSerializer3<string> enumerableSerializer = new IEnumerableSerializer3<string>(stringSerializer);

            DefaultClassSerializer3 defaultClassSerializer = new DefaultClassSerializer3(integerSerializer, enumerableSerializer, stringSerializer);
            byte[] buffer = new byte[defaultClassSerializer.Count(instance)];
            int writeIndex = 0;
            defaultClassSerializer.Serialize(instance, buffer, ref writeIndex);
            int readIndex = 0;
            Class newInstance = defaultClassSerializer.Deserialize(buffer, ref readIndex);
        }
    }

    public class Class
    {
        public int Field;
        public List<string> Strings { get; set; }
        public int IntProperty { get; set; }
        public string StringProperty { get; set; }
    }



    public class IntegerSerializer1 : ISerializer1<int>
    {
        public int Deserialize(byte[] bytes)
        {
            int value = 0;
            for (int i = 0; i != bytes.Length; i++)
                value |= bytes[i] << (3 - i) * 8;
            return value;
        }
        public byte[] Serialize(int instance)
        {
            int length = 4;
            byte[] bytes = new byte[length];
            for (int i = 0; i != bytes.Length; i++)
                bytes[i] = (byte)(instance >> (3 - i) * 8);
            return bytes;
        }
    }
    public class ClassSerializer1 : ISerializer1<Class>
    {
        public Class Deserialize(byte[] bytes)
        {
            Class instance = new Class();
            for (int i = 0; i != 4; i++)
            {
                instance.Field |= bytes[i] << (3 - i) * 8;
                instance.IntProperty |= bytes[i + 4] << (3 - i) * 8;
            }
            return instance;
        }
        public byte[] Serialize(Class instance)
        {
            int length = 8;
            byte[] bytes = new byte[length];

            for (int i = 0; i != 4; i++)
            {
                bytes[i] = (byte)(instance.Field >> (3 - i) * 8);
                bytes[i + 4] = (byte)(instance.IntProperty >> (3 - i) * 8);
            }
            return bytes;
        }
    }
    public class StringSerializer1 : ISerializer1<string>
    {
        public string Deserialize(byte[] bytes) => Encoding.UTF8.GetString(bytes);
        public byte[] Serialize(string instance) => Encoding.UTF8.GetBytes(instance);
    }
    public interface ISerializer1<T>
    {
        byte[] Serialize(T instance);
        T Deserialize(byte[] bytes);
    }

    public class IntegerSerializer2 : ISerializer2<int>
    {
        public int Deserialize(byte[] buffer, ref int index)
        {
            int value = 0;
            value |= buffer[index++] << 24;
            value |= buffer[index++] << 16;
            value |= buffer[index++] << 8;
            value |= buffer[index++];
            return value;
        }
        public void Serialize(int instance, byte[] buffer, ref int index)
        {
            buffer[index++] = (byte)(instance >> 24);
            buffer[index++] = (byte)(instance >> 16);
            buffer[index++] = (byte)(instance >> 8);
            buffer[index++] = (byte)instance;
        }
    }
    public class StringSerializer2 : ISerializer2<string>
    {
        public string Deserialize(byte[] buffer, ref int index)
        {
            int instanceLength = 0;
            instanceLength |= buffer[index++] << 24;
            instanceLength |= buffer[index++] << 16;
            instanceLength |= buffer[index++] << 8;
            instanceLength |= buffer[index++];
            string result = Encoding.UTF8.GetString(buffer, index, instanceLength);
            index += instanceLength;
            return result;
        }
        public void Serialize(string instance, byte[] buffer, ref int index)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(instance);

            int instanceLength = bytes.Length;
            buffer[index++] = (byte)(instanceLength >> 24);
            buffer[index++] = (byte)(instanceLength >> 16);
            buffer[index++] = (byte)(instanceLength >> 8);
            buffer[index++] = (byte)instanceLength;

            for (int i = 0; i < instanceLength; i++)
                buffer[index++] = bytes[i];
        }
    }
    public class IEnumerableSerializer2<T> : ISerializer2<IEnumerable<T>>
    {
        private readonly ISerializer2<T> _serializer;

        public IEnumerableSerializer2(ISerializer2<T> serializer) => _serializer = serializer;

        public void Serialize(IEnumerable<T> instance, byte[] buffer, ref int index)
        {
            IEnumerator<T> enumerator = instance.GetEnumerator();
            int countIndex = index;
            index += 4;

            if (!enumerator.MoveNext())
                return;

            int elementCount = 1;
            T item;
            for (item = enumerator.Current; enumerator.MoveNext(); item = enumerator.Current, elementCount++)
                _serializer.Serialize(item, buffer, ref index);
            _serializer.Serialize(item, buffer, ref index);

            buffer[countIndex++] = (byte)(elementCount >> 24);
            buffer[countIndex++] = (byte)(elementCount >> 16);
            buffer[countIndex++] = (byte)(elementCount >> 8);
            buffer[countIndex++] = (byte)elementCount;
        }
        public IEnumerable<T> Deserialize(byte[] buffer, ref int index)
        {
            int instanceLength = 0;
            instanceLength |= buffer[index++] << 24;
            instanceLength |= buffer[index++] << 16;
            instanceLength |= buffer[index++] << 8;
            instanceLength |= buffer[index++];

            T[] tArray = new T[instanceLength];

            for (int i = 0; i != instanceLength; i++)
                tArray[i] = _serializer.Deserialize(buffer, ref index);

            return tArray;
        }
    }
    public interface ISerializer2<T>
    {
        void Serialize(T instance, byte[] buffer, ref int index);
        T Deserialize(byte[] buffer, ref int index);
    }

    public class IntegerSerializer3 : ISerializer3<int>
    {
        public int Count(int instance) => 4;
        public int Deserialize(byte[] buffer, ref int index)
        {
            int value = 0;
            value |= buffer[index++] << 24;
            value |= buffer[index++] << 16;
            value |= buffer[index++] << 8;
            value |= buffer[index++];
            return value;
        }
        public void Serialize(int instance, byte[] buffer, ref int index)
        {
            buffer[index++] = (byte)(instance >> 24);
            buffer[index++] = (byte)(instance >> 16);
            buffer[index++] = (byte)(instance >> 8);
            buffer[index++] = (byte)instance;
        }
    }
    public class StringSerializer3 : ISerializer3<string>
    {
        public int Count(string instance) => 4 + Encoding.UTF8.GetByteCount(instance);
        public string Deserialize(byte[] buffer, ref int index)
        {
            int instanceLength = 0;
            instanceLength |= buffer[index++] << 24;
            instanceLength |= buffer[index++] << 16;
            instanceLength |= buffer[index++] << 8;
            instanceLength |= buffer[index++];
            string result = Encoding.UTF8.GetString(buffer, index, instanceLength);
            index += instanceLength;
            return result;
        }
        public void Serialize(string instance, byte[] buffer, ref int index)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(instance);

            int instanceLength = bytes.Length;
            buffer[index++] = (byte)(instanceLength >> 24);
            buffer[index++] = (byte)(instanceLength >> 16);
            buffer[index++] = (byte)(instanceLength >> 8);
            buffer[index++] = (byte)instanceLength;

            for (int i = 0; i < instanceLength; i++)
                buffer[index++] = bytes[i];
        }
    }
    public class IEnumerableSerializer3<T> : ISerializer3<IEnumerable<T>>
    {
        private readonly ISerializer3<T> _serializer;

        public IEnumerableSerializer3(ISerializer3<T> serializer) => _serializer = serializer;

        public int Count(IEnumerable<T> instance)
        {
            IEnumerator<T> enumerator = instance.GetEnumerator();
            if (!enumerator.MoveNext())
                return 4;

            int count = 4;
            T item;
            for (item = enumerator.Current; enumerator.MoveNext(); item = enumerator.Current)
                count += _serializer.Count(item);
            count += _serializer.Count(item);
            return count;
        }
        public void Serialize(IEnumerable<T> instance, byte[] buffer, ref int index)
        {
            IEnumerator<T> enumerator = instance.GetEnumerator();
            int countIndex = index;
            index += 4;

            if (!enumerator.MoveNext())
                return;

            int elementCount = 1;
            T item;
            for (item = enumerator.Current; enumerator.MoveNext(); item = enumerator.Current, elementCount++)
                _serializer.Serialize(item, buffer, ref index);
            _serializer.Serialize(item, buffer, ref index);

            buffer[countIndex++] = (byte)(elementCount >> 24);
            buffer[countIndex++] = (byte)(elementCount >> 16);
            buffer[countIndex++] = (byte)(elementCount >> 8);
            buffer[countIndex++] = (byte)elementCount;
        }
        public IEnumerable<T> Deserialize(byte[] buffer, ref int index)
        {
            int instanceLength = 0;
            instanceLength |= buffer[index++] << 24;
            instanceLength |= buffer[index++] << 16;
            instanceLength |= buffer[index++] << 8;
            instanceLength |= buffer[index++];

            T[] tArray = new T[instanceLength];

            for (int i = 0; i != instanceLength; i++)
                tArray[i] = _serializer.Deserialize(buffer, ref index);

            return tArray;
        }
    }
    public class DefaultClassSerializer3 : ISerializer3<Class>
    {
        private readonly ISerializer3<int> _integerSerializer;
        private readonly ISerializer3<IEnumerable<string>> _enumerableSerializer;
        private readonly ISerializer3<string> _stringSerializer;

        public DefaultClassSerializer3(ISerializer3<int> integerSerializer, ISerializer3<IEnumerable<string>> listSerializer, ISerializer3<string> stringSerializer)
        {
            _integerSerializer = integerSerializer ?? throw new ArgumentNullException(nameof(integerSerializer));
            _enumerableSerializer = listSerializer ?? throw new ArgumentNullException(nameof(listSerializer));
            _stringSerializer = stringSerializer ?? throw new ArgumentNullException(nameof(stringSerializer));
        }

        public int Count(Class instance) => _integerSerializer.Count(instance.Field) + _enumerableSerializer.Count(instance.Strings) + _integerSerializer.Count(instance.IntProperty) + _stringSerializer.Count(instance.StringProperty);

        public void Serialize(Class instance, byte[] buffer, ref int index)
        {
            _integerSerializer.Serialize(instance.Field, buffer, ref index);
            _enumerableSerializer.Serialize(instance.Strings, buffer, ref index);
            _integerSerializer.Serialize(instance.IntProperty, buffer, ref index);
            _stringSerializer.Serialize(instance.StringProperty, buffer, ref index);
        }

        public Class Deserialize(byte[] buffer, ref int index)
        {
            Class instance = new Class
            {
                Field = _integerSerializer.Deserialize(buffer, ref index),
                Strings = _enumerableSerializer.Deserialize(buffer, ref index).ToList(),
                IntProperty = _integerSerializer.Deserialize(buffer, ref index),
                StringProperty = _stringSerializer.Deserialize(buffer, ref index)
            };
            return instance;
        }
    }
    public interface ISerializer3<T>
    {
        int Count(T instance);
        void Serialize(T instance, byte[] buffer, ref int index);
        T Deserialize(byte[] buffer, ref int index);
    }

    public interface ISerializer<T>
    {
        int Count(T instance);
        void Serialize(T instance, byte[] buffer, ref int index);
        T Deserialize(byte[] buffer, ref int index);
    }

    static public class SerializerBuilder
    {
        public ISerializer<T> Create<T>(SerializerBuilderSettings<T> serializerBuilderSettings)
        {

        }
    }

    internal class MemberSerializerInfo<T>
    {
        public MemberInfo Member;
        public object Serializer;

        public MemberSerializerInfo(MemberInfo member, object serializer)
        {
            Member = member ?? throw new ArgumentNullException(nameof(member));
            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }
    }

    public abstract class SerializerBuilder<T>
    {
        private readonly List<MemberSerializerInfo<T>> _membersInformation;

        public SerializerBuilder()
        {
            _membersInformation = new List<MemberSerializerInfo<T>>();
            Build(new SerializerBuilderSettings<T>(_membersInformation));
        }

        public abstract void Build(SerializerBuilderSettings<T> serializerBuilderSettings);
    }

    public class SerializerBuilderSettings<T>
    {
        private readonly List<MemberSerializerInfo<T>> _membersInformation;

        internal SerializerBuilderSettings(object membersInformation) => _membersInformation = (List<MemberSerializerInfo<T>>)membersInformation;

        public void AddMember<TMember>(Expression<Func<T, TMember>> member, object serializer) => _membersInformation.Add(new MemberSerializerInfo<T>(member.Body, serializer));
    }

    public class Target
    {
        public int Integer { get; set; }
        public string Str { get; set; }
    }

    public class TargetSerializer : SerializerBuilder<Target>
    {
        public override void Build(SerializerBuilderSettings<Target> serializerBuilderSettings)
        {
            serializerBuilderSettings.AddMember(x => x.Integer, new IntegerSerializer());
            serializerBuilderSettings.AddMember(x => x.Str, new StringSerializer());
        }
    }



    public abstract class Serializer<T> : ISerializer<T>
    {
        static public ISerializer<T> Default;

        static Serializer() => Default =

        public abstract int Count(T instance);
        public abstract void Serialize(T instance, byte[] buffer, ref int index);
        public abstract T Deserialize(byte[] buffer, ref int index);
    }
    public abstract class ContantLenghtSerializer<T> : ISerializer<T>
    {
        static private int _count;

        protected ContantLenghtSerializer(int count) => _count = count;

        static public int Count => _count;
        public abstract T Deserialize(byte[] buffer, ref int index);
        public abstract void Serialize(T instance, byte[] buffer, ref int index);

        int ISerializer<T>.Count(T instance) => _count;
    }
    public class StringSerializer : Serializer<string>
    {
        public override int Count(string instance) => IntegerSerializer.Count + Encoding.UTF8.GetByteCount(instance);
        public override string Deserialize(byte[] buffer, ref int index) => throw new NotImplementedException();
        public override void Serialize(string instance, byte[] buffer, ref int index) => throw new NotImplementedException();
    }
    public class IntegerSerializer : ContantLenghtSerializer<int>
    {
        public IntegerSerializer() : base(sizeof(int)) { }

        public override void Serialize(int instance, byte[] buffer, ref int index)
        {
            buffer[index++] = (byte)(instance >> 24);
            buffer[index++] = (byte)(instance >> 16);
            buffer[index++] = (byte)(instance >> 8);
            buffer[index++] = (byte)instance;
        }
        public override int Deserialize(byte[] buffer, ref int index)
        {
            int value = 0;
            value |= buffer[index++] << 24;
            value |= buffer[index++] << 16;
            value |= buffer[index++] << 8;
            value |= buffer[index++];
            return value;
        }
    }
}