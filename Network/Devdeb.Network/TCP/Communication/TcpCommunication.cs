using Devdeb.Serialization;
using Devdeb.Serialization.Serializers.System;
using Devdeb.Sets.Extensions;
using System;
using System.Net.Sockets;

namespace Devdeb.Network.TCP.Communication
{
    public class TcpCommunication
    {
        private class Buffer
        {
            /// <remarks>WARNING: Closed for write or update.</remarks>
            public byte[] Data;
            public int Offset;
            public int Count;
            public readonly object DataLock;

            public Buffer(int bufferSize)
            {
                if (bufferSize <= 0)
                    throw new ArgumentException($"The {nameof(bufferSize)} must be grater 0.");
                Data = new byte[bufferSize];
                DataLock = new object();
            }

            public int MovedAvailableSpace => Data.Length - Count;
            public int UnmovedAvailableSpace => Data.Length - Offset - Count;

            public void Add(byte[] buffer, int offset, int count)
            {
                if (buffer == null)
                    throw new ArgumentNullException(nameof(buffer));
                if (offset < 0)
                    throw new ArgumentOutOfRangeException(nameof(offset));
                if (count <= 0)
                    throw new ArgumentOutOfRangeException(nameof(count));
                if (offset + count > buffer.Length)
                    throw new Exception($"The {nameof(offset)} with {nameof(count)} exceeds {nameof(buffer.Length)}");

                EnsureAvailableSpace(count);
                lock (DataLock)
                {
                    Array.Copy(buffer, offset, Data, Offset + Count, count);
                    Count += count;
                }
            }
            public void AddWithSize(byte[] buffer, int offset, int count)
            {
                if (buffer == null)
                    throw new ArgumentNullException(nameof(buffer));
                if (offset < 0)
                    throw new ArgumentOutOfRangeException(nameof(offset));
                if (count <= 0)
                    throw new ArgumentOutOfRangeException(nameof(count));
                if (offset + count > buffer.Length)
                    throw new Exception($"The {nameof(offset)} with {nameof(count)} exceeds {nameof(buffer.Length)}");

                EnsureAvailableSpace(count);
                lock (DataLock)
                {
                    int startOffset = Offset + Count;
                    Int32Serializer.Default.Serialize(count, Data, ref startOffset);
                    Array.Copy(buffer, offset, Data, startOffset, count);
                    Count += Int32Serializer.Default.Size + count;
                }
            }
            public void Add<T>(ISerializer<T> serializer, T instance)
            {
                if (serializer == null)
                    throw new ArgumentNullException(nameof(serializer));

                int instanceSize = serializer.Size(instance);
                EnsureAvailableSpace(instanceSize);
                lock (DataLock)
                {
                    serializer.Serialize(instance, Data, Offset + Count);
                    Count += instanceSize;
                }
            }
            public void AddWithSize<T>(ISerializer<T> serializer, T instance)
            {
                if (serializer == null)
                    throw new ArgumentNullException(nameof(serializer));

                int instanceSize = serializer.Size(instance);
                EnsureAvailableSpace(instanceSize);
                lock (DataLock)
                {
                    int startOffset = Offset + Count;
                    Int32Serializer.Default.Serialize(instanceSize, Data, ref startOffset);
                    serializer.Serialize(instance, Data, startOffset);
                    Count += Int32Serializer.Default.Size + instanceSize;
                }
            }

            public void Read(byte[] buffer, int offset, int count)
            {
                if (buffer == null)
                    throw new ArgumentNullException(nameof(buffer));
                if (offset < 0)
                    throw new ArgumentOutOfRangeException(nameof(offset));
                if (count <= 0)
                    throw new ArgumentOutOfRangeException(nameof(count));
                if (offset + count > buffer.Length)
                    throw new Exception($"The {nameof(offset)} with {nameof(count)} exceeds {nameof(buffer.Length)}.");
                if (count > Count)
                    throw new ArgumentOutOfRangeException($"Requested {nameof(count)}: {count} exceeds available {Count}.");

                lock (DataLock)
                {
                    Array.Copy(Data, Offset, buffer, offset, count);

                    Count -= count;
                    Offset = Count == 0 ? 0 : Offset + count;
                }
            }
            public T Read<T>(ISerializer<T> serializer, int count)
            {
                if (serializer == null)
                    throw new ArgumentNullException(nameof(serializer));
                if (count <= 0)
                    throw new ArgumentOutOfRangeException(nameof(count));
                if (count > Count)
                    throw new ArgumentOutOfRangeException($"Requested count: {count} exceeds available {Count}.");

                T result;
                lock (DataLock)
                {
                    bool needCount = serializer.Flags.HasFlag(SerializerFlags.NeedCount);
                    result = serializer.Deserialize(Data, Offset, needCount ? (int?)count : null);
                    Count -= count;
                    Offset = Count == 0 ? 0 : Offset + count;
                }
                return result;
            }

            public void EnsureAvailableSpace(int count)
            {
                if (count < 0)
                    throw new ArgumentOutOfRangeException(nameof(count));

                lock (DataLock)
                {
                    for (; MovedAvailableSpace < count;)
                        ArrayExtensions.IncreaseLength(ref Data);

                    if (UnmovedAvailableSpace < count)
                    {
                        Array.Copy(Data, Offset, Data, 0, Count);
                        Offset = 0;
                    }
                }
            }
        }

        private readonly Socket _tcpSocket;
        private readonly Buffer _receivingBuffer;
        private readonly Buffer _sendingBuffer;
        private readonly object _receivingLock;
        private readonly object _sendingLock;

        public TcpCommunication(Socket tcpSocket)
        {
            _tcpSocket = tcpSocket ?? throw new ArgumentNullException(nameof(tcpSocket));
            if (_tcpSocket.ProtocolType != ProtocolType.Tcp)
                throw new Exception($"{nameof(tcpSocket)} has no tcp protocol type.");

            _tcpSocket.Blocking = false;
            _sendingBuffer = new Buffer(tcpSocket.SendBufferSize);
            _receivingBuffer = new Buffer(tcpSocket.ReceiveBufferSize);
            _receivingLock = new object();
            _sendingLock = new object();
        }

        public Socket Socket => _tcpSocket;
        public int ReceivedBytesCount => _receivingBuffer.Count + _tcpSocket.Available;
        public int BufferBytesCount => _receivingBuffer.Count;

        public void Receive(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (offset + count > buffer.Length)
                throw new Exception($"The {nameof(offset)} with {nameof(count)} exceeds {nameof(buffer.Length)}");
            if (ReceivedBytesCount < count)
                throw new Exception($"Requesting bytes count {count} exceeds received {ReceivedBytesCount}.");

            if (_receivingBuffer.Count >= count)
            {
                _receivingBuffer.Read(buffer, offset, count);
                return;
            }

            int receivedBytesCount = 0;
            SocketError socketError = SocketError.Success;
            lock (_receivingLock)
            {
                int receivingBufferCount = _receivingBuffer.Count;
                if (receivingBufferCount != 0)
                {
                    _receivingBuffer.Read(buffer, offset, receivingBufferCount);
                    count -= receivingBufferCount;

                    if (count == 0)
                        return;

                    offset += receivingBufferCount;
                }
                receivedBytesCount = _tcpSocket.Receive(buffer, offset, count, SocketFlags.None, out socketError);
            }

            if (socketError != SocketError.Success && socketError != SocketError.WouldBlock)
                Shutdown(socketError);

            if (receivedBytesCount != count)
                throw new Exception("Couldn't read the data in the requested amount.");
        }
        public T Receive<T>(ISerializer<T> serializer, int count) => _receivingBuffer.Read(serializer, count);
        public T Receive<T>(IConstantLengthSerializer<T> serializer) => Receive(serializer, serializer.Size);
        public void Send(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (offset + count > buffer.Length)
                throw new Exception($"The {nameof(offset)} with {nameof(count)} exceeds {nameof(buffer.Length)}");

            SocketError socketError = SocketError.Success;
            int sentBytesCount = 0;

            lock (_sendingLock)
                if (_sendingBuffer.Count == 0)
                    sentBytesCount = _tcpSocket.Send(buffer, offset, count, SocketFlags.None, out socketError);

            if (socketError != SocketError.Success && socketError != SocketError.WouldBlock)
                Shutdown(socketError);

            if (sentBytesCount == count)
                return;

            _sendingBuffer.Add(buffer, offset + sentBytesCount, count - sentBytesCount);
        }
        public void SendWithSize(byte[] buffer, int offset, int count) => _sendingBuffer.AddWithSize(buffer, offset, count);
        public void Send<T>(ISerializer<T> serializer, T instance) => _sendingBuffer.Add(serializer, instance);
        public void SendWithSize<T>(ISerializer<T> serializer, T instance) => _sendingBuffer.AddWithSize(serializer, instance);

        public void ReceiveToBuffer()
        {
            if (_tcpSocket.Available == 0)
                return;

            SocketError socketError = SocketError.Success;
            lock (_receivingLock)
            {
                lock (_receivingBuffer.DataLock)
                {
                    int readCount = _tcpSocket.Available;
                    _receivingBuffer.EnsureAvailableSpace(readCount);

                    int receivedBytesCount = _tcpSocket.Receive(
                        _receivingBuffer.Data,
                        _receivingBuffer.Offset,
                        readCount,
                        SocketFlags.None,
                        out socketError
                    );

                    _receivingBuffer.Count += receivedBytesCount;
                }
            }

            if (socketError != SocketError.Success && socketError != SocketError.WouldBlock)
                Shutdown(socketError);
        }
        public void SendBuffer()
        {
            if (_sendingBuffer.Count == 0)
                return;

            lock (_sendingBuffer.DataLock)
            {
                SocketError socketError = SocketError.Success;
                int sentBytesCount = 0;

                lock (_sendingLock)
                {
                    sentBytesCount = _tcpSocket.Send(
                        _sendingBuffer.Data,
                        _sendingBuffer.Offset,
                        _sendingBuffer.Count,
                        SocketFlags.None,
                        out socketError
                    );
                }

                if (socketError != SocketError.Success && socketError != SocketError.WouldBlock)
                    Shutdown(socketError);

                _sendingBuffer.Count -= sentBytesCount;
                _sendingBuffer.Offset = _sendingBuffer.Count == 0 ? 0 : _sendingBuffer.Offset + sentBytesCount;
            }
        }

        public void Shutdown(SocketError socketError) => throw new Exception($"SocketError: {socketError}");
        public void Shutdown() { }
    }
}
