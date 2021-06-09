using Devdeb.DependencyInjection.Tests.Domain.Abstractions;

namespace Devdeb.DependencyInjection.Tests.Domain
{
	public class UniversityService : IUniversityService
	{
		static private int _initializaionNumber;
		static private readonly object _initializaionNumberLocker = new object();

		public UniversityService()
		{
			lock (_initializaionNumberLocker)
				InitializaionNumber = ++_initializaionNumber;
		}

		public string GetUniversityName => "BMSTU";

		public int InitializaionNumber { get; }
	}
}
