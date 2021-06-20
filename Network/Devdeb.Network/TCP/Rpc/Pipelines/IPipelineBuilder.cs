namespace Devdeb.Network.TCP.Rpc.Pipelines
{
	public interface IPipelineBuilder
	{
		void Use(MiddlewareDelegate requestDelegate);
		PipelineEntryPointDelegate Build();
	}
}
