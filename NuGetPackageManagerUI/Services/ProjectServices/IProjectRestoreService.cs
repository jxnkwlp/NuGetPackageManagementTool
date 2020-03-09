using NuGet.ProjectManagement.Projects;
using NuGet.ProjectModel;
using NuGetPackageManagerUI.MsBuild;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.Services.ProjectServices
{
	public interface IProjectRestoreService : ISingletonService
	{
		Task RestoreAsync(BuildIntegratedNuGetProject project);

		Task<DependencyGraphSpec> GetOrCreateDependencyGraphSpecAsync(IProjectAdapter projectAdapter, BuildIntegratedNuGetProject project);
	}
}
