using NuGet.Configuration;
using NuGet.PackageManagement;
using NuGet.ProjectManagement;
using NuGetPackageManagerUI.MsBuild;
using NuGetPackageManagerUI.Services.NuGets.Projects;
using NuGetPackageManagerUI.Services.NuGets.ProjectServices;
using NuGetPackageManagerUI.Services.ProjectServices;
using NuGetPackageManagerUI.Services.ProjectSystems;
using NuGetPackageManagerUI.VisualStudio;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.Services.Projects
{
	public class MSBuildNuGetProjectProvider : INuGetProjectProvider
	{
		public async Task<NuGetProject> TryCreateNuGetProjectAsync(IProjectAdapter projectAdapter, ProjectProviderContext context, IProjectRestoreService projectRestoreService, bool forceProjectType)
		{
			var projectSystem = await MSBuildNuGetProjectSystemFactory.CreateMSBuildNuGetProjectSystemAsync(projectAdapter, context.ProjectContext);

			await projectSystem.InitializeAsync();

			var projectServices = new VsMSBuildProjectSystemServices(new MSBuildProjectSystem(projectAdapter, context.ProjectContext), projectAdapter);

			var solutionDirectory = projectAdapter.SolutionDirectory ?? projectAdapter.ProjectDirectory;

			var nuGetSettings = Settings.LoadDefaultSettings(solutionDirectory);

			var folderNuGetProjectFullPath = PackagesFolderPathUtility.GetPackagesFolderPath(solutionDirectory, nuGetSettings);

			var packagesConfigFolderPath = projectAdapter.ProjectDirectory;

			return new NetFrameworkNuGetProject(projectSystem, 
							folderNuGetProjectFullPath, 
							packagesConfigFolderPath, 
							projectServices);
		}
	}
}
