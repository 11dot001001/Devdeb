namespace Devdeb.DependencyInjection.Tests.Domain.Abstractions
{
	public interface IUniversityService
	{
		string GetUniversityName { get; }
		int InitializaionNumber { get; }
	}
}
