using NuGet.ProjectManagement;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.VisualStudio
{
	public class MsNuGetProjectProvider : INuGetProjectProvider
	{
		public async Task<NuGetProject> TryCreateNuGetProjectAsync(MSProjectManager mSProjectManager, ProjectProviderContext context, bool forceProjectType)
		{
			var projectSystem = await MSBuildNuGetProjectSystemFactory.CreateMSBuildNuGetProjectSystemAsync(mSProjectManager, context.ProjectContext);

			var projectServices = new VsMSBuildProjectSystemServices(projectSystem, mSProjectManager);

			var folderNuGetProjectFullPath = context.PackagesPathFactory();

			var packagesConfigFolderPath = mSProjectManager.ProjectDirectory;

			var nuGetProject = new MSBuildNuGetProject(projectSystem, folderNuGetProjectFullPath, packagesConfigFolderPath);

			return nuGetProject;
		}
	}
}
