using System.Threading.Tasks;
using IServiceProvider = Devdeb.DependencyInjection.IServiceProvider;

namespace Devdeb.Network.TCP.Rpc.Pipelines
{
	public delegate Task PipelineEntryPointDelegate(IServiceProvider serviceProvider);

	public delegate Task NextMiddlewareDelegate();

	public delegate Task MiddlewareDelegate(IServiceProvider serviceProvider, NextMiddlewareDelegate nextMiddleware);
}
