namespace Devdeb.Network.TCP.Rpc.Handler
{
	internal delegate void ProcessRequest<THandler>(byte[] buffer, ref int offset, THandler handler);
}
