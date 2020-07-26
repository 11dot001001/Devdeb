using Devdeb.Network.Connection;
using System;

namespace Devdeb.Network.TCP.Connection
{
	public class TCPConnectionPackage : IConnectionPackage
	{
		public enum CreatingBytesAction
		{
			CopyDataWithLenght,
			CopyData,
			CopyReference,
		}

		public const int PackageDataCountCapacity = sizeof(int);

		public unsafe TCPConnectionPackage(byte[] bytes, CreatingBytesAction createBytesAction)
		{
			if (bytes == null)
				throw new ArgumentNullException(nameof(bytes));
			switch (createBytesAction)
			{
				case CreatingBytesAction.CopyDataWithLenght:
				{
					Data = new byte[bytes.Length + PackageDataCountCapacity];
					fixed (byte* dataCountPointer = &Data[0])
						*(int*)dataCountPointer = bytes.Length;
					Array.Copy(bytes, 0, Data, PackageDataCountCapacity, bytes.Length);
					break;
				}
				case CreatingBytesAction.CopyData:
				{
					Data = new byte[bytes.Length];
					Array.Copy(bytes, 0, Data, 0, bytes.Length);
					break;
				}
				case CreatingBytesAction.CopyReference:
				{
					Data = bytes;
					break;
				}
				default:
					break;
			}
		}
		public unsafe TCPConnectionPackage(int packageDataLenght) => Data = new byte[packageDataLenght];

		public byte[] Data { get; }
	}
}