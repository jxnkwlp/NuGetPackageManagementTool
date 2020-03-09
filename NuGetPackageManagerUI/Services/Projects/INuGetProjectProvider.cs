using NuGet.ProjectManagement;
using NuGetPackageManagerUI.MsBuild;
using NuGetPackageManagerUI.Services.NuGets.ProjectServices;
using NuGetPackageManagerUI.Services.ProjectServices;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.Services.Projects
{
	public interface INuGetProjectProvider : ISingletonService
	{
		Task<NuGetProject> TryCreateNuGetProjectAsync(IProjectAdapter projectAdapter, ProjectProviderContext context, IProjectRestoreService projectRestoreService, bool forceProjectType);
	}
}
