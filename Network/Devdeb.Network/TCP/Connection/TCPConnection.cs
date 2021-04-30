using System;
using System.Net.Sockets;

namespace Devdeb.Network.TCP.Connection
{
	//public class TCPConnection
	//{
	//	private class Package
	//	{
	//		public unsafe Package(byte[] data)
	//		{
	//			Data = data;
	//			fixed (byte* dataLenghtPointer = &DataLenght[0])
	//				*(int*)dataLenghtPointer = data.Length;
	//		}

	//		public byte[] DataLenght { get; }
	//		public int DataLenghtOffset { get; set; }
	//		public byte[] Data { get; }
	//		public int DataOffset { get; set; }
	//	}

	//	private readonly Socket _tcpConnection;
	//	private readonly Queue<byte[]> _waitingToSendBuffers;
	//	private readonly Queue<byte[]> _receivedBuffers;
	//	private Package _sendingPackage;
	//	private Package _receivingPackage;

	//	public TCPConnection(Socket tcpConnection)
	//	{
	//		if (tcpConnection.ProtocolType != ProtocolType.Tcp)
	//			throw new Exception($"{nameof(tcpConnection)} has no tcp protocol type.");
	//		_tcpConnection = tcpConnection ?? throw new ArgumentNullException(nameof(tcpConnection));
	//		_waitingToSendBuffers = new Queue<byte[]>();
	//		_receivedBuffers = new Queue<byte[]>();
	//	}

	//	public int ReceivedBufferCount => _receivedBuffers.Count;

	//	public void AddBufferToSend(byte[] buffer)
	//	{
	//		if (buffer == null)
	//			throw new ArgumentNullException(nameof(buffer));
	//		_waitingToSendBuffers.Enqueue(buffer);
	//	}
	//	public byte[] ReceiveBuffer() => _receivedBuffers.Dequeue();

	//	private void SendBytes()
	//	{

	//	}
	//}

	public class TcpCommunication
	{
		private class Package
		{
			public const int BufferLengthCapacity = sizeof(int);

			public unsafe Package(byte[] buffer)
			{
				Buffer = buffer;
				BufferLength = new byte[BufferLengthCapacity];
				fixed (byte* bufferLenghtPointer = &BufferLength[0])
					*(int*)bufferLenghtPointer = buffer.Length;
			}
			public Package(int bufferLength) => Buffer = new byte[bufferLength];

			public byte[] Buffer { get; set; }
			public byte[] BufferLength { get; set; }
			public int BufferOffset { get; set; }
			public int BufferLengthOffset { get; set; }

			public bool IsBufferLengthAwaiting => (BufferLengthCapacity ^ BufferLengthOffset) != 0;
			public bool IsBufferAwaiting => (Buffer.Length ^ BufferLengthOffset) != 0;
		}
		private struct SendBytesResult
		{
			public SendBytesResult(int sentBytesCount, int remainingBytesCount)
			{
				SentBytesCount = sentBytesCount;
				RemainingBytesCount = remainingBytesCount;
			}

			public int SentBytesCount { get; }
			public int RemainingBytesCount { get; }
		}

		private readonly Socket _tcpSocket;
		private Package _sendingPackage;
		private Package _receivingPackage;

		public TcpCommunication(Socket tcpSocket)
		{
			if (tcpSocket.ProtocolType != ProtocolType.Tcp)
				throw new Exception($"{nameof(tcpSocket)} has no tcp protocol type.");
			_tcpSocket = tcpSocket ?? throw new ArgumentNullException(nameof(tcpSocket));
		}

		public bool IsSendingBufferContained => _sendingPackage != null;
		public bool IsReceivingBufferContained => _receivingPackage != null;

		public void AddBufferToSend(byte[] buffer)
		{
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			_sendingPackage = new Package(buffer);
		}
		public byte[] ReceiveBuffer()
		{
			if (_receivingPackage == null)
				throw new Exception("Communication doesn't contain received buffer.");
			byte[] receivedBuffer = _receivingPackage.Buffer;
			_receivingPackage = null;
			return receivedBuffer;
		}
		public void Communicate()
		{

		}

		private void SendBytes()
		{
			if (_sendingPackage == null)
				return;

			SendBytesResult sendBytesResult;
			if (_sendingPackage.BufferLengthOffset != Package.BufferLengthCapacity)
			{
				sendBytesResult = SendBytes(_sendingPackage.BufferLength, _sendingPackage.BufferLengthOffset);
				_sendingPackage.BufferLengthOffset += sendBytesResult.SentBytesCount;
				if (_sendingPackage.IsBufferLengthAwaiting)
					return;
			}

			sendBytesResult = SendBytes(_sendingPackage.Buffer, _sendingPackage.BufferOffset);
			_sendingPackage.BufferOffset += sendBytesResult.SentBytesCount;
			if (_sendingPackage.IsBufferLengthAwaiting)
				return;
			_sendingPackage = null;
		}
		private unsafe void ReceiveBytes()
		{
			if (_tcpSocket.Available == 0)
				return;
			int receivedBytesCount;
			SocketError socketError;
			if (_receivingPackage == null)
			{
				if (_tcpSocket.Available < Package.BufferLengthCapacity)
					return;
				byte[] packetBufferLength = new byte[Package.BufferLengthCapacity];
				receivedBytesCount = _tcpSocket.Receive
				(
					buffer: packetBufferLength,
					offset: 0,
					size: Package.BufferLengthCapacity,
					socketFlags: SocketFlags.None, 
					errorCode: out socketError
				);
				if (socketError != SocketError.Success)
					throw new Exception($"{nameof(SocketError)} is {socketError}.");
				if(receivedBytesCount != Package.BufferLengthCapacity)
					throw new Exception($"Recived count of package length {receivedBytesCount} is invalid. Expected {Package.BufferLengthCapacity}.");
				int bufferLength;
				fixed (byte* packetBufferLengthPointer = &packetBufferLength[0])
					bufferLength = *(int*)packetBufferLengthPointer;
				_receivingPackage = new Package(bufferLength);
			}

			int remainingBytesCount = _receivingPackage.Buffer.Length - _receivingPackage.BufferOffset;
			receivedBytesCount = _tcpSocket.Receive
			(
				buffer: _receivingPackage.Buffer,
				offset: _receivingPackage.BufferOffset,
				size: remainingBytesCount,
				socketFlags: SocketFlags.None,
				errorCode: out socketError
			);
			if (socketError != SocketError.Success)
				throw new Exception($"{nameof(SocketError)} is {socketError}");
			_receivingPackage.BufferOffset += receivedBytesCount;
			remainingBytesCount = _receivingPackage.Buffer.Length - _receivingPackage.BufferOffset;
			if (remainingBytesCount != 0)
				return;
		}

		private SendBytesResult SendBytes(byte[] buffer, int offset)
		{
			int remainingBytesCount = buffer.Length - offset;
			int sentBytesCount = _tcpSocket.Send
			(
				buffer: buffer,
				offset: offset,
				size: remainingBytesCount,
				socketFlags: SocketFlags.None,
				errorCode: out SocketError socketError
			);
			if (socketError != SocketError.Success)
				throw new Exception($"{nameof(SocketError)} is {socketError}.");
			SendBytesResult result = new SendBytesResult
			(
				sentBytesCount: sentBytesCount,
				remainingBytesCount: buffer.Length - offset + sentBytesCount
			);
			return result;
		}
	}
} 