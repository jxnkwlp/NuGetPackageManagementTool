using NuGet.Configuration;
using NuGet.PackageManagement;
using NuGet.ProjectManagement;

namespace NuGetPackageManagerUI.VisualStudio
{
	public class NuGetProjectFactory
	{
		public static NuGetProject CreateNuGetProject(ISolutionManager solutionManager, MSProjectManager projectManager, INuGetProjectContext nuGetProjectContext)
		{
			if (projectManager.GetProjectTypeGuid() != VsProjectTypes.CsharpProjectTypeGuid)
			{
				// unsupport
				return null;
			}

			VsMSBuildProjectSystem projectSystem = new VsMSBuildProjectSystem(projectManager, nuGetProjectContext);

			var targetFrameworkIdentifier = projectManager.GetTargetFrameworkIdentifier();
			var targetFramework = projectManager.GetTargetFramework();
			var projectName = projectManager.GetMSBuildProjectName();
			var nuGetProjectStyle = projectManager.GetNuGetProjectStyle();

			if (nuGetProjectStyle == "PackageReference")
			{
				return CreatePackageReferenceNuGetProject(solutionManager, projectSystem, projectManager);
			}
			else if (nuGetProjectStyle == "PackagesConfig")
			{
				return CreateMSBuildNuGetProject(solutionManager, projectSystem, projectManager);
			}

			if (targetFrameworkIdentifier == ".NETFramework")
			{
				return CreateMSBuildNuGetProject(solutionManager, projectSystem, projectManager);
			}
			else if (targetFrameworkIdentifier == ".NETCoreApp")
			{
				return CreatePackageReferenceNuGetProject(solutionManager, projectSystem, projectManager);
			}

			return null;
		}

		private static NuGetProject CreatePackageReferenceNuGetProject(ISolutionManager solutionManager, VsMSBuildProjectSystem projectSystem, MSProjectManager projectManager)
		{
			//var services = new VsMSBuildProjectSystemServices(projectSystem, projectManager);
			//return new NetCorePackageReferenceProject(projectName, projectName, projectManager.ProjectFullPath, null, services);
			return null;
		}

		private static NuGetProject CreateMSBuildNuGetProject(ISolutionManager solutionManager, VsMSBuildProjectSystem projectSystem, MSProjectManager projectManager)
		{
			var nuGetSettings = Settings.LoadDefaultSettings(solutionManager.SolutionDirectory);

			var packagesFolderPath = PackagesFolderPathUtility.GetPackagesFolderPath(solutionManager, nuGetSettings);

			return new MSBuildNuGetProject(projectSystem, packagesFolderPath, projectManager.ProjectDirectory);
		}
	}
}
