using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IServiceProvider = Devdeb.DependencyInjection.IServiceProvider;

namespace Devdeb.Network.TCP.Rpc.Pipelines
{
	internal class PipelineBuilder : IPipelineBuilder
	{
		private readonly Queue<MiddlewareDelegate> _middlewareQueue;

		public PipelineBuilder() => _middlewareQueue = new Queue<MiddlewareDelegate>();

		public void Use(MiddlewareDelegate requestDelegate)
		{
			_middlewareQueue.Enqueue(requestDelegate ?? throw new ArgumentNullException(nameof(requestDelegate)));
		}

		public PipelineEntryPointDelegate Build()
		{
			return GetNextMiddleware() ?? ((IServiceProvider serviceProvider) => Task.CompletedTask);
		}

		private PipelineEntryPointDelegate GetNextMiddleware()
		{
			if (!_middlewareQueue.TryDequeue(out MiddlewareDelegate middleware))
				return null;

			PipelineEntryPointDelegate nextMiddlewareDelegate = GetNextMiddleware();

			if (nextMiddlewareDelegate == null)
				return (IServiceProvider serviceProvider) => middleware.Invoke(serviceProvider, null);

			return (IServiceProvider serviceProvider) =>
			{
				return middleware.Invoke(serviceProvider, () => nextMiddlewareDelegate(serviceProvider));
			};
		}
	}
}
