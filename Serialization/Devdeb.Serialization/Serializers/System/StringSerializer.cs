using System;
using System.Text;

namespace Devdeb.Serialization.Serializers.System
{
    public sealed class StringSerializer : ISerializer<string>
    {
        static StringSerializer()
        {
            Default = new StringSerializer(Encoding.Default);
            ASCII = new StringSerializer(Encoding.ASCII);
            UTF7 = new StringSerializer(Encoding.UTF7);
            UTF8 = new StringSerializer(Encoding.UTF8);
            UTF32 = new StringSerializer(Encoding.UTF32);
            Unicode = new StringSerializer(Encoding.Unicode);
            BigEndianUnicode = new StringSerializer(Encoding.BigEndianUnicode);
        }

        static public StringSerializer Default { get; }
        static public StringSerializer ASCII { get; }
        static public StringSerializer UTF7 { get; }
        static public StringSerializer UTF8 { get; }
        static public StringSerializer UTF32 { get; }
        static public StringSerializer Unicode { get; }
        static public StringSerializer BigEndianUnicode { get; }

        private readonly Encoding _encoding;

        public Encoding Encoding => _encoding;

        public StringSerializer(Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        public int GetSize(string instance) => _encoding.GetByteCount(instance);

        public void Serialize(string instance, Span<byte> buffer)
        {
            int writtenByteCount = _encoding.GetBytes(instance, buffer); ;
            if (writtenByteCount != GetSize(instance))
                throw new Exception($"The {nameof(writtenByteCount)} doesn't match the {nameof(instance)} size.");
        }

        public string Deserialize(ReadOnlySpan<byte> buffer) => _encoding.GetString(buffer);
    }
}
