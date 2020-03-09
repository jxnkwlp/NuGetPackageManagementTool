using NuGet.Commands;
using NuGet.Common;
using NuGet.Frameworks;
using NuGet.PackageManagement;
using NuGet.ProjectManagement;
using NuGet.RuntimeModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.MsBuild
{
	public class ProjectAdapter : IProjectAdapter
	{
		public ProjectAdapter(MsBuildProject msBuildProject)
		{
			Project = msBuildProject;
			BuildProperties = new CustomProjectBuildProperties(msBuildProject);
		}

		public ProjectAdapter(MsBuildProject msBuildProject, ISolutionManager solutionManager)
		{
			Project = msBuildProject;
			BuildProperties = new CustomProjectBuildProperties(msBuildProject);

			SolutionManager = solutionManager;
		}

		public Task InitialAsync()
		{
			ProjectName = Project.GetProjectName();
			ProjectUniqueName = Project.GetProjectUniqueName();
			ProjectFilePath = Project.ProjectFilePath;
			ProjectDirectory = Project.ProjectDirectory;

			return Task.CompletedTask;
		}


		public MsBuildProject Project { get; }

		public IProjectBuildProperties BuildProperties { get; }

		public ISolutionManager SolutionManager { get; set; }

		public string ProjectId => null;

		public string ProjectName { get; private set; }

		public string ProjectUniqueName { get; private set; }

		public string ProjectFilePath { get; private set; }

		public string ProjectDirectory { get; private set; }

		public string SolutionDirectory => SolutionManager.SolutionDirectory;

		public bool IsSupported { get; private set; }

		public string PackageTargetFallback { get; private set; }

		public async Task<FrameworkName> GetDotNetFrameworkNameAsync()
		{
			var targetFrameworkMoniker = await GetTargetFrameworkStringAsync();

			if (!string.IsNullOrEmpty(targetFrameworkMoniker))
			{
				return new FrameworkName(targetFrameworkMoniker);
			}

			return null;
		}

		public Task<string> GetNuGetLockFilePathAsync()
		{
			throw new NotImplementedException();
		}

		public async Task<string[]> GetProjectTypeGuidsAsync()
		{
			var typeGuid = Project.GetProjectTypeGuids();

			var projectTypeGuids = await BuildProperties.GetPropertyValueAsync(ProjectBuildProperties.ProjectTypeGuids);

			if (!string.IsNullOrEmpty(projectTypeGuids))
			{
				return MSBuildStringUtility.Split(projectTypeGuids);
			}
			else if (!string.IsNullOrEmpty(typeGuid))
			{
				return MSBuildStringUtility.Split(typeGuid);
			}

			return Array.Empty<string>();
		}

		public Task<IEnumerable<string>> GetReferencedProjectsAsync()
		{
			throw new NotImplementedException();
		}

		public Task<string> GetRestorePackagesWithLockFileAsync()
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<RuntimeDescription>> GetRuntimeIdentifiersAsync()
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<CompatibilityProfile>> GetRuntimeSupportsAsync()
		{
			throw new NotImplementedException();
		}

		public async Task<NuGetFramework> GetTargetFrameworkAsync()
		{
			var frameworkString = await GetTargetFrameworkStringAsync();

			if (!string.IsNullOrEmpty(frameworkString))
			{
				return NuGetFramework.Parse(frameworkString);
			}

			return NuGetFramework.UnsupportedFramework;
		}


		public Task<bool> IsRestoreLockedAsync()
		{
			throw new NotImplementedException();
		}

		private Task<string> GetTargetFrameworkStringAsync()
		{
			var frameworkStrings = MSBuildProjectFrameworkUtility.GetProjectFrameworkStrings(
						projectFilePath: ProjectFilePath,
						targetFrameworks: BuildProperties.GetPropertyValue("TargetFrameworks"),
						targetFramework: BuildProperties.GetPropertyValue("TargetFramework"),
						targetFrameworkMoniker: BuildProperties.GetPropertyValue("TargetFrameworkMoniker"),
						targetPlatformIdentifier: BuildProperties.GetPropertyValue("TargetPlatformIdentifier"),
						targetPlatformVersion: BuildProperties.GetPropertyValue("TargetPlatformVersion"),
						targetPlatformMinVersion: BuildProperties.GetPropertyValue("TargetPlatformMinVersion"));

			return Task.FromResult(frameworkStrings.FirstOrDefault());
		}
	}
}
