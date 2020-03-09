using NuGet.ProjectManagement;
using NuGet.ProjectModel;
using NuGetPackageManagerUI.MsBuild;
using NuGetPackageManagerUI.Services.NuGets.ProjectServices;
using NuGetPackageManagerUI.Services.ProjectServices;
using NuGetPackageManagerUI.VisualStudio;
using System;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.Services.Projects
{
	public class NetCorePackageReferenceProjectProvider : INuGetProjectProvider
	{
		private static readonly string PackageReference = ProjectStyle.PackageReference.ToString();

		public async Task<NuGetProject> TryCreateNuGetProjectAsync(IProjectAdapter projectAdapter, ProjectProviderContext context, IProjectRestoreService projectRestoreService, bool forceProjectType)
		{
			var buildProperties = projectAdapter.BuildProperties;

			// read MSBuild property RestoreProjectStyle, TargetFramework, and TargetFrameworks
			var restoreProjectStyle = await buildProperties.GetPropertyValueAsync(ProjectBuildProperties.RestoreProjectStyle);
			var targetFramework = await buildProperties.GetPropertyValueAsync(ProjectBuildProperties.TargetFramework);
			var targetFrameworks = await buildProperties.GetPropertyValueAsync(ProjectBuildProperties.TargetFrameworks);

			// check for RestoreProjectStyle property is set and if not set to PackageReference then return false
			if (!(string.IsNullOrEmpty(restoreProjectStyle) ||
				restoreProjectStyle.Equals(PackageReference, StringComparison.OrdinalIgnoreCase)))
			{
				return null;
			}
			// check whether TargetFramework or TargetFrameworks property is set, else return false
			else if (string.IsNullOrEmpty(targetFramework) && string.IsNullOrEmpty(targetFrameworks))
			{
				return null;
			}

			//var projectName = projectAdapter.ProjectName;
			//var fullProjectPath = projectAdapter.ProjectFilePath;

			var projectServices = new VsMSBuildProjectSystemServices(new MSBuildProjectSystem(projectAdapter, context.ProjectContext), projectAdapter);

			return new NetCorePackageReferenceNuGetProject(projectAdapter, projectServices, projectRestoreService);
		}
	}
}
