using Devdeb.Network.Connection;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Devdeb.Network.TCP.Connection
{
	public class TCPConnectionProvider : IConnectionProvider<TCPConnectionPackage>
	{
		private class ReceivingPackage : TCPConnectionPackage
		{
			public ReceivingPackage(int packageDataLenght) : base(packageDataLenght) => DataLenght = packageDataLenght;

			public int DataLenght { get; }
			public int Offset { get; set; }
			public int ResidualQuantity => Data.Length - Offset;
			public bool IsAccumulated => ResidualQuantity == 0;
		}
		private class SendingPackage : TCPConnectionPackage
		{
			public SendingPackage(byte[] data) : base(data, CreatingBytesAction.CopyReference) { }

			public int Offset { get; set; }
			public int ResidualQuantity => Data.Length - Offset;
			public bool IsSent => Data.Length == Offset;
		}

		public const int PackageDataCountCapacity = sizeof(int);

		private readonly Socket _tcpConnection;
		private readonly Queue<TCPConnectionPackage> _sendingPackages;
		private readonly Queue<TCPConnectionPackage> _receivedPackages;
		private SendingPackage _sendingPackage;
		private ReceivingPackage _receivingPackage;

		public TCPConnectionProvider(Socket tcpConnection)
		{
			_tcpConnection = tcpConnection ?? throw new ArgumentNullException(nameof(tcpConnection));
			_sendingPackages = new Queue<TCPConnectionPackage>();
			_receivedPackages = new Queue<TCPConnectionPackage>();
		}

		public Socket Connection => _tcpConnection;
		public int SendingPackagesCount => _sendingPackages.Count;
		public int ReceivedPackagesCount => _receivedPackages.Count;

		public void SendBytes()
		{
			if (_sendingPackage == null)
			{
				if (SendingPackagesCount == 0)
					return;
				_sendingPackage = new SendingPackage(_sendingPackages.Dequeue().Data);
			}
			int sentBytesCount = _tcpConnection.Send(_sendingPackage.Data, _sendingPackage.Offset, _sendingPackage.ResidualQuantity, SocketFlags.None, out SocketError socketError);
			if (socketError != SocketError.Success)
				throw new Exception($"{nameof(SocketError)} is {socketError}");
			_sendingPackage.Offset += sentBytesCount;
			if (_sendingPackage.IsSent)
				_sendingPackage = null;
		}
		public unsafe void ReceiveBytes()
		{
			if (_tcpConnection.Available == 0)
				return;
			SocketError socketError;
			if (_receivingPackage == null)
			{
				if (_tcpConnection.Available < PackageDataCountCapacity)
					return;
				byte[] packageDataLenght = new byte[PackageDataCountCapacity];
				int receivedCount = _tcpConnection.Receive(packageDataLenght, 0, PackageDataCountCapacity, SocketFlags.None, out socketError);
				if (socketError != SocketError.Success)
					throw new Exception($"{nameof(SocketError)} is {socketError}");
				if (receivedCount != PackageDataCountCapacity)
					throw new Exception($"Recived count of package length {receivedCount} is invalid. Expected {PackageDataCountCapacity}");
				fixed (byte* packageDataLenghtPointer = &packageDataLenght[0])
					_receivingPackage = new ReceivingPackage(*(int*)packageDataLenghtPointer);
			}
			int recivedBytesCount = _tcpConnection.Receive(_receivingPackage.Data, _receivingPackage.Offset, _receivingPackage.ResidualQuantity, SocketFlags.None, out socketError);
			if (socketError != SocketError.Success)
				throw new Exception($"{nameof(SocketError)} is {socketError}");
			_receivingPackage.Offset += recivedBytesCount;
			if (_receivingPackage.IsAccumulated)
			{
				_receivedPackages.Enqueue(_receivingPackage);
				_receivingPackage = null;
			}
		}

		public void AddPackageToSend(TCPConnectionPackage package) => _sendingPackages.Enqueue(package);
		public TCPConnectionPackage GetPackage()
		{
			if (ReceivedPackagesCount == 0)
				throw new Exception("Connection doesn't contain received package.");
			return _receivedPackages.Dequeue();
		}
	
		public void Close()
		{
			_tcpConnection.Shutdown(SocketShutdown.Both);
			_tcpConnection.Close();
		}
	}
}