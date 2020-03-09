using NuGet.PackageManagement;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.MsBuild
{
	public interface IProjectAdapterProvider : ISingletonService
	{
		Task<IProjectAdapter> GetOrCreateAdapterAsync(string projectFilePath, ISolutionManager solutionManager = null);
		Task<IProjectAdapter> GetOrCreateAdapterAsync(MsBuildProject msBuildProject, ISolutionManager solutionManager = null);
	}
}
