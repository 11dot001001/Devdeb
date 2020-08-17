using Devdeb.Network.Connection;
using System;

namespace Devdeb.Network.TCP.Connection
{
	public class TCPConnectionPackage : IConnectionPackage
	{
		public enum CreatingBytesAction
		{
			CopyData,
			CopyReference,
		}

		public const int PackageTypeCapacity = sizeof(ConnectionPackageType);
		public const int PackageDataCountCapacity = sizeof(int);
		public const int PackageServiceInfoCapacity = PackageTypeCapacity + PackageDataCountCapacity;

		public unsafe TCPConnectionPackage(byte[] serviceBuffer)
		{
			Type = (ConnectionPackageType)serviceBuffer[0];
			fixed (byte* dataLenghtPointer = &serviceBuffer[1])
				DataLenght = *(int*)dataLenghtPointer;
			Data = new byte[DataLenght];
		}
		public TCPConnectionPackage(ConnectionPackageType type, byte[] bytes, CreatingBytesAction createBytesAction)
		{
			if (bytes == null)
				throw new ArgumentNullException(nameof(bytes));
			switch (createBytesAction)
			{
				case CreatingBytesAction.CopyData:
				{
					Data = new byte[bytes.Length];
					Array.Copy(bytes, 0, Data, 0, bytes.Length);
					break;
				}
				case CreatingBytesAction.CopyReference:
				{
					Data = bytes;
					DataLenght = bytes.Length;
					break;
				}
				default:
					throw new ArgumentOutOfRangeException(nameof(type));
			}
			Type = type;
		}
		public TCPConnectionPackage(ConnectionPackageType type, int packageDataLenght)
		{
			Type = type;
			DataLenght = packageDataLenght;
			Data = new byte[packageDataLenght];
		}

		public ConnectionPackageType Type { get; }
		public int DataLenght { get; }
		public byte[] Data { get; }

		public unsafe byte[] GetServiceData()
		{
			byte[] serviceBuffer = new byte[PackageServiceInfoCapacity];
			serviceBuffer[0] = (byte)Type;
			fixed (byte* dataLenghtPointer = &serviceBuffer[1])
				*(int*)dataLenghtPointer = DataLenght;
			return serviceBuffer;
		}
	}
}