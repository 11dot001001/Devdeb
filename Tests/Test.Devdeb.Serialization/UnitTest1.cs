using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Devdeb.Serialization
{
    [TestClass]
    public class SerializationTest
    {
        [TestMethod]
        public void TestMethod()
        {
            string str = "helloHELLOприветПРИВЕТ123"; // 25 length - 12 russian - 10 english - 3 numbers
            int integer = 121212233;
            StringSerializer2 stringSerializer = new StringSerializer2();
            IntegerSerializer2 integerSerializer2 = new IntegerSerializer2();

            byte[] buffer = new byte[1024];
            int writeOffset = 0;
            stringSerializer.Serialize(str, buffer, ref writeOffset);
            integerSerializer2.Serialize(integer, buffer, ref writeOffset);

            int readOffset = 0;
            string str2 = stringSerializer.Deserialize(buffer, ref readOffset);
            int integer2 = integerSerializer2.Deserialize(buffer, ref readOffset);


            IntegerSerializer1 integerSerializer = new IntegerSerializer1();
            int a = -2;
            byte[] b = integerSerializer.Serialize(a);
            int z = integerSerializer.Deserialize(b);

            Class instance = new Class() { Field = 323, Property = 42443234 };
            ClassSerializer1 classSerializer = new ClassSerializer1();
            byte[] bytes = classSerializer.Serialize(instance);
            Class instance2 = classSerializer.Deserialize(bytes);
        }
    }

    public class Class
    {
        public int Field;
        public int Property { get; set; }
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
                instance.Property |= bytes[i + 4] << (3 - i) * 8;
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
                bytes[i + 4] = (byte)(instance.Property >> (3 - i) * 8);
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

    public interface ISerializer2<T>
    {
        void Serialize(T instance, byte[] buffer, ref int index);
        T Deserialize(byte[] buffer, ref int index);
    }
}