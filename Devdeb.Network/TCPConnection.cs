using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Devdeb.Network
{
	internal class TCPConnection
	{
		private class ReceivingPackage
		{
			public ReceivingPackage(int expectedCount)
			{
				ExpectedResidualQuantity = expectedCount;
				Data = new byte[expectedCount];
			}

			public byte[] Data { get; }
			public int ExpectedResidualQuantity { get; set; }
			public bool IsAccumulated => ExpectedResidualQuantity == 0;
		}

		private const int PackageLengthCapacity = sizeof(int);

		private readonly Socket _connection;
		private readonly Queue<byte[]> _packagesToSend;
		private int _sendBufferOffset;
		private readonly Queue<ReceivingPackage> _receivedPackages;
		private ReceivingPackage _receivingPackage;

		public TCPConnection(Socket connection)
		{
			_connection = connection ?? throw new ArgumentNullException(nameof(connection));
			_packagesToSend = new Queue<byte[]>();
			_receivedPackages = new Queue<ReceivingPackage>();
		}

		internal void SendBytes()
		{
			if (_packagesToSend.Count == 0)
				return;
			byte[] sendingBytes = _packagesToSend.Peek();
			int sentBytesCount = _connection.Send(sendingBytes, _sendBufferOffset, sendingBytes.Length - _sendBufferOffset, SocketFlags.None, out SocketError socketError);
			if (socketError != SocketError.Success)
				throw new Exception($"{nameof(SocketError)} is {socketError}");
			_sendBufferOffset += sentBytesCount;
			if (_sendBufferOffset == sendingBytes.Length)
			{
				_ = _packagesToSend.Dequeue();
				_sendBufferOffset = 0;
			}
		}
		internal unsafe void ReceiveBytes()
		{
			if (_connection.Available == 0)
				return;
			SocketError socketError;
			if (_receivingPackage == null)
			{
				if (_connection.Available < PackageLengthCapacity)
					return;
				byte[] packageLengthBytes = new byte[sizeof(int)];
				int receivedCount = _connection.Receive(packageLengthBytes, 0, 4, SocketFlags.None, out socketError);
				if (socketError != SocketError.Success)
					throw new Exception($"{nameof(SocketError)} is {socketError}");
				if (receivedCount != sizeof(int))
					throw new Exception($"Recived count of package length {receivedCount} is invalid. Expected {PackageLengthCapacity}");

				int packageLength;
				fixed (byte* pointer = &packageLengthBytes[0])
					packageLength = *(int*)pointer;
				_receivingPackage = new ReceivingPackage(packageLength);
			}
			int recivedBytesCount = _connection.Receive(_receivingPackage.Data, _receivingPackage.Data.Length - _receivingPackage.ExpectedResidualQuantity, _receivingPackage.ExpectedResidualQuantity, SocketFlags.None, out socketError);
			if (socketError != SocketError.Success)
				throw new Exception($"{nameof(SocketError)} is {socketError}");
			_receivingPackage.ExpectedResidualQuantity -= recivedBytesCount;
			if (_receivingPackage.IsAccumulated)
			{
				_receivedPackages.Enqueue(_receivingPackage);
				_receivingPackage = null;
			}
		}

		public unsafe void SendPackage(byte[] bytes)
		{
			byte[] packageBytes = new byte[bytes.Length + 4];
			fixed (byte* pointer = &packageBytes[0])
				*(int*)pointer = bytes.Length;
			Array.Copy(bytes, 0, packageBytes, sizeof(int), bytes.Length);
			_packagesToSend.Enqueue(packageBytes);
		}
		public bool TryReceivePackage(out byte[] bytes)
		{
			bytes = null;
			if (_receivedPackages.Count == 0)
				return false;
			bytes = _receivedPackages.Dequeue().Data;
			return true;
		}
	}
}