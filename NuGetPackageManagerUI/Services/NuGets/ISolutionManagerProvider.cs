using NuGet.PackageManagement;

namespace NuGetPackageManagerUI.Services.NuGets
{
	public interface ISolutionManagerProvider : ISingletonService
	{
		ISolutionManager CreateOrGetSolutionManager();
	}
}
