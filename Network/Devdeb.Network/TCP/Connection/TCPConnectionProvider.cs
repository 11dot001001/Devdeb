using Devdeb.Network.Connection;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Devdeb.Network.TCP.Connection
{
	public class TCPConnectionProvider : IConnectionProvider<TCPConnectionPackage>
	{
		private class AwaitingPackage : TCPConnectionPackage
		{
			public AwaitingPackage(byte[] serviceBuffer) : base(serviceBuffer) { }
			public AwaitingPackage(ConnectionPackageType type, byte[] buffer) : base(type, buffer, CreatingBytesAction.CopyReference) { }

			public int ServiceDataOffset { get; set; }
			public int ServiceDataResidualQuantity => PackageServiceInfoCapacity - ServiceDataOffset;
			public bool IsServiceDataAwaiting => ServiceDataResidualQuantity != 0;
			public int DataOffset { get; set; }
			public int DataResidualQuantity => DataLenght - DataOffset;
			public bool IsDataAwaiting => DataResidualQuantity != 0;
		}

		private readonly Socket _tcpConnection;
		private readonly Queue<TCPConnectionPackage> _sendingPackages;
		private readonly Queue<TCPConnectionPackage> _receivedPackages;
		private readonly Queue<TCPConnectionPackage> _sendingServicePackages;
		private readonly Queue<TCPConnectionPackage> _receivedServicePackages;
		private AwaitingPackage _sendingPackage;
		private AwaitingPackage _receivingPackage;

		public TCPConnectionProvider(Socket tcpConnection)
		{
			_tcpConnection = tcpConnection ?? throw new ArgumentNullException(nameof(tcpConnection));
			_sendingPackages = new Queue<TCPConnectionPackage>();
			_receivedPackages = new Queue<TCPConnectionPackage>();
			_sendingServicePackages = new Queue<TCPConnectionPackage>();
			_receivedServicePackages = new Queue<TCPConnectionPackage>();
		}

		public Socket Connection => _tcpConnection;
		public int SendingPackagesCount => _sendingPackages.Count;
		public int ReceivedPackagesCount => _receivedPackages.Count;
		public int SendingServicePackagesCount => _sendingServicePackages.Count;
		public int ReceivedServicePackagesCount => _receivedServicePackages.Count;
		public bool HasReceivedPackages => (ReceivedPackagesCount | ReceivedServicePackagesCount) != 0;

		public void SendBytes()
		{
			if (_sendingPackage == null)
			{
				TCPConnectionPackage tcpConnectionPackage = null;
				if ((SendingServicePackagesCount | SendingPackagesCount) == 0)
					return;
				if(SendingServicePackagesCount != 0)
					tcpConnectionPackage = _sendingServicePackages.Dequeue();
				else if (SendingPackagesCount != 0)
					tcpConnectionPackage = _sendingPackages.Dequeue();
				_sendingPackage = new AwaitingPackage(tcpConnectionPackage.Type, tcpConnectionPackage.Data);
			}
			SocketError socketError;
			int sentBytesCount;
			if (_sendingPackage.IsServiceDataAwaiting)
			{
				byte[] serviceDataBuffer = _sendingPackage.GetServiceData();
				sentBytesCount = _tcpConnection.Send(serviceDataBuffer, _sendingPackage.ServiceDataOffset, _sendingPackage.ServiceDataResidualQuantity, SocketFlags.None, out socketError);
				if (socketError != SocketError.Success)
					throw new Exception($"{nameof(SocketError)} is {socketError}.");
				_sendingPackage.ServiceDataOffset += sentBytesCount;
				if (_sendingPackage.IsServiceDataAwaiting)
					return;
			}
			sentBytesCount = _tcpConnection.Send(_sendingPackage.Data, _sendingPackage.DataOffset, _sendingPackage.DataResidualQuantity, SocketFlags.None, out socketError);
			if (socketError != SocketError.Success)
				throw new Exception($"{nameof(SocketError)} is {socketError}.");
			_sendingPackage.DataOffset += sentBytesCount;
			if (!_sendingPackage.IsDataAwaiting)
				_sendingPackage = null;
		}
		public unsafe void ReceiveBytes()
		{
			if (_tcpConnection.Available == 0)
				return;
			SocketError socketError;
			if (_receivingPackage == null)
			{
				if (_tcpConnection.Available < TCPConnectionPackage.PackageServiceInfoCapacity)
					return;

				byte[] serrviceBuffer = new byte[TCPConnectionPackage.PackageServiceInfoCapacity];
				int receivedCount = _tcpConnection.Receive(serrviceBuffer, 0, TCPConnectionPackage.PackageServiceInfoCapacity, SocketFlags.None, out socketError);
				if (socketError != SocketError.Success)
					throw new Exception($"{nameof(SocketError)} is {socketError}.");
				if (receivedCount != TCPConnectionPackage.PackageServiceInfoCapacity)
					throw new Exception($"Recived count of package length {receivedCount} is invalid. Expected {TCPConnectionPackage.PackageServiceInfoCapacity}.");
				_receivingPackage = new AwaitingPackage(serrviceBuffer);
			}

			int recivedBytesCount = _tcpConnection.Receive(_receivingPackage.Data, _receivingPackage.DataOffset, _receivingPackage.DataResidualQuantity, SocketFlags.None, out socketError);
			if (socketError != SocketError.Success)
				throw new Exception($"{nameof(SocketError)} is {socketError}");
			_receivingPackage.DataOffset += recivedBytesCount;
			if (!_receivingPackage.IsDataAwaiting)
			{
				if (_receivingPackage.Type == ConnectionPackageType.User)
					_receivedPackages.Enqueue(_receivingPackage);
				else
					_receivedServicePackages.Enqueue(_receivingPackage);
				_receivingPackage = null;
			}
		}

		public void AddPackageToSend(TCPConnectionPackage package)
		{
			switch (package.Type)
			{
				case ConnectionPackageType.Service:
					_sendingPackages.Enqueue(package);
					break;
				case ConnectionPackageType.User:
					_sendingServicePackages.Enqueue(package);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(package.Type));
			}
		}
		public TCPConnectionPackage GetPackage()
		{
			if (ReceivedPackagesCount == 0)
				throw new Exception("Connection doesn't contain received package.");
			return _receivedPackages.Dequeue();
		}
		public TCPConnectionPackage GetServicePackage()
		{
			if (ReceivedServicePackagesCount == 0)
				throw new Exception("Connection doesn't contain received package.");
			return _receivedServicePackages.Dequeue();
		}
	}
}