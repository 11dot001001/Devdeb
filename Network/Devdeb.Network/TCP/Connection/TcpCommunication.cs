using Devdeb.Sets.Extensions;
using System;
using System.Net.Sockets;

namespace Devdeb.Network.TCP.Connection
{
    public class TcpCommunication
    {
        private class Buffer
        {
            /// <remarks>WARNING: Closed for write or update.</remarks>
            public byte[] Data;
            public int Offset;
            public int Count;
            public readonly object DataLocker;

            public Buffer(int bufferSize)
            {
                if (bufferSize <= 0)
                    throw new ArgumentException($"The {nameof(bufferSize)} must be grater 0.");
                Data = new byte[bufferSize];
                DataLocker = new object();
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

                lock (DataLocker)
                {
                    for (; MovedAvailableSpace < count;)
                        ArrayExtensions.IncreaseLength(ref Data);

                    if (UnmovedAvailableSpace >= count)
                    {
                        Array.Copy(buffer, offset, Data, Offset, count);
                        Count += count;
                    }
                    else if (MovedAvailableSpace >= count)
                    {
                        Array.Copy(Data, Offset, Data, 0, Count);
                        Array.Copy(buffer, offset, Data, Count, count);
                        Offset = 0;
                        Count += count;
                    }
                }
            }
        }

        private readonly Socket _tcpSocket;
        private readonly Buffer _receivingBuffer;
        private readonly Buffer _sendingBuffer;
        private readonly object _sendingLock;

        public TcpCommunication(Socket tcpSocket)
        {
            _tcpSocket = tcpSocket ?? throw new ArgumentNullException(nameof(tcpSocket));
            if (_tcpSocket.ProtocolType != ProtocolType.Tcp)
                throw new Exception($"{nameof(tcpSocket)} has no tcp protocol type.");

            _tcpSocket.Blocking = false;
            _sendingBuffer = new Buffer(tcpSocket.SendBufferSize);
            _sendingLock = new object();
        }

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

            if (socketError != SocketError.Success || socketError != SocketError.WouldBlock)
                Shutdown(socketError);

            if (sentBytesCount == count)
                return;

            Add(buffer, offset + sentBytesCount, count - sentBytesCount);
        }
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

            _sendingBuffer.Add(buffer, offset, count);
        }
        public void SendBuffer()
        {
            lock (_sendingBuffer.DataLocker)
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

                if (socketError != SocketError.Success || socketError != SocketError.WouldBlock)
                    Shutdown(socketError);

                _sendingBuffer.Count -= sentBytesCount;

                if (_sendingBuffer.Count == 0)
                    _sendingBuffer.Offset = 0;
                else
                    _sendingBuffer.Offset += sentBytesCount;
            }
        }

        public void Shutdown(SocketError socketError) => throw new Exception($"SocketError: {socketError}");
        public void Shutdown() { }
    }
}
