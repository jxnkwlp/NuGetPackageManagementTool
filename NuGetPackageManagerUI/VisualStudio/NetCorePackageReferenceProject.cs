using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.ProjectSystem.References;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.LibraryModel;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.ProjectManagement.Projects;
using NuGet.ProjectModel;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PackageReference = NuGet.Packaging.PackageReference;

namespace NuGetPackageManagerUI.VisualStudio
{
	public class NetCorePackageReferenceProject : BuildIntegratedNuGetProject
	{
		private const string TargetFrameworkCondition = "TargetFramework";

		private readonly string _projectName;
		private readonly string _projectUniqueName;
		private readonly string _projectFullPath;

		private readonly UnconfiguredProject _unconfiguredProject;

		public NetCorePackageReferenceProject(
			string projectName,
			string projectUniqueName,
			string projectFullPath,
			UnconfiguredProject unconfiguredProject,
			INuGetProjectServices projectServices,
			string projectId)
		{
			_projectName = projectName;
			_projectUniqueName = projectUniqueName;
			_projectFullPath = projectFullPath;

			ProjectStyle = ProjectStyle.PackageReference;

			_unconfiguredProject = unconfiguredProject;
			ProjectServices = projectServices;

			InternalMetadata.Add(NuGetProjectMetadataKeys.Name, _projectName);
			InternalMetadata.Add(NuGetProjectMetadataKeys.UniqueName, _projectUniqueName);
			InternalMetadata.Add(NuGetProjectMetadataKeys.FullPath, _projectFullPath);
			InternalMetadata.Add(NuGetProjectMetadataKeys.ProjectId, projectId);
		}

		public override async Task<string> GetAssetsFilePathAsync()
		{
			return await GetAssetsFilePathAsync(shouldThrow: true);
		}

		public override async Task<string> GetAssetsFilePathOrNullAsync()
		{
			return await GetAssetsFilePathAsync(shouldThrow: false);
		}

		public override Task AddFileToProjectAsync(string filePath)
		{
			// sdk-style project system uses globbing to dynamically add files from project root into project
			// so we dont need to do anything explicitly here.
			return Task.CompletedTask;
		}

		private Task<string> GetAssetsFilePathAsync(bool shouldThrow)
		{
			var packageSpec = GetPackageSpec();
			if (packageSpec == null)
			{
				if (shouldThrow)
				{
					throw new InvalidOperationException(string.Format("ProjectNotLoaded_RestoreFailed {0}", ProjectName));
				}
				else
				{
					return Task.FromResult<string>(null);
				}
			}

			return Task.FromResult(Path.Combine(
				packageSpec.RestoreMetadata.OutputPath,
				LockFileFormat.AssetsFileName));
		}

		private PackageSpec GetPackageSpec()
		{
			//if (_projectSystemCache.TryGetProjectRestoreInfo(_projectFullPath, out DependencyGraphSpec projectRestoreInfo))
			//{
			//	return projectRestoreInfo.GetProjectSpec(_projectFullPath);
			//}

			// if restore data was not found in the cache, meaning project nomination
			// didn't happen yet or failed.
			return null;
		}


		#region IDependencyGraphProject


		public override string MSBuildProjectPath => _projectFullPath;

		public override string ProjectName => _projectName;

		public override Task<IReadOnlyList<PackageSpec>> GetPackageSpecsAsync(DependencyGraphCacheContext context)
		{
			var projects = new List<PackageSpec>();

			DependencyGraphSpec projectRestoreInfo = new DependencyGraphSpec();

			//if (!_projectSystemCache.TryGetProjectRestoreInfo(_projectFullPath, out DependencyGraphSpec projectRestoreInfo))
			//{
			//	throw new InvalidOperationException(string.Format("ProjectNotLoaded_RestoreFailed {0}", ProjectName));
			//}

			// Apply ISettings when needed to the return values.
			// This should not change the cached specs since they
			// contain values such as CLEAR which need to be persisted
			// and used here.
			var originalProjects = projectRestoreInfo.Projects;

			var settings = context?.Settings ?? NullSettings.Instance;

			foreach (var originalProject in originalProjects)
			{
				var project = originalProject.Clone();

				// Read restore settings from ISettings if it doesn't exist in the project
				// NOTE: Very important that the original project is used in the arguments, because cloning sorts the sources and compromises how the sources will be evaluated
				project.RestoreMetadata.PackagesPath = RestoreSettingsUtilities.GetPackagesPath(settings, originalProject);
				project.RestoreMetadata.Sources = RestoreSettingsUtilities.GetSources(settings, originalProject);
				project.RestoreMetadata.FallbackFolders = RestoreSettingsUtilities.GetFallbackFolders(settings, originalProject);
				project.RestoreMetadata.ConfigFilePaths = GetConfigFilePaths(settings);
				IgnoreUnsupportProjectReference(project);
				projects.Add(project);
			}

			if (context != null)
			{
				foreach (var project in projects
					.Where(p => !context.PackageSpecCache.TryGetValue(
						p.RestoreMetadata.ProjectUniqueName, out PackageSpec ignore)))
				{
					context.PackageSpecCache.Add(
						project.RestoreMetadata.ProjectUniqueName,
						project);
				}
			}

			return Task.FromResult<IReadOnlyList<PackageSpec>>(projects);
		}

		private IList<string> GetConfigFilePaths(ISettings settings)
		{
			return settings.GetConfigFilePaths();
		}

		private void IgnoreUnsupportProjectReference(PackageSpec project)
		{
			foreach (var frameworkInfo in project.RestoreMetadata.TargetFrameworks)
			{
				var projectReferences = new List<ProjectRestoreReference>();

				foreach (var projectReference in frameworkInfo.ProjectReferences)
				{
					if (SupportedProjectTypes.IsSupportedProjectExtension(projectReference.ProjectPath))
					{
						projectReferences.Add(projectReference);
					}
				}

				frameworkInfo.ProjectReferences = projectReferences;
			}
		}

		#endregion

		#region NuGetProject

		public override Task<IEnumerable<PackageReference>> GetInstalledPackagesAsync(CancellationToken token)
		{
			PackageReference[] installedPackages;

			var packageSpec = GetPackageSpec();
			if (packageSpec != null)
			{
				installedPackages = GetPackageReferences(packageSpec);
			}
			else
			{
				installedPackages = new PackageReference[0];
			}

			return Task.FromResult<IEnumerable<PackageReference>>(installedPackages);
		}

		private static PackageReference[] GetPackageReferences(PackageSpec packageSpec)
		{
			var frameworkSorter = new NuGetFrameworkSorter();

			return packageSpec
				.TargetFrameworks
				.SelectMany(f => GetPackageReferences(f.Dependencies, f.FrameworkName))
				.GroupBy(p => p.PackageIdentity)
				.Select(g => g.OrderBy(p => p.TargetFramework, frameworkSorter).First())
				.ToArray();
		}

		private static IEnumerable<PackageReference> GetPackageReferences(IEnumerable<LibraryDependency> libraries, NuGetFramework targetFramework)
		{
			return libraries
				.Where(l => l.LibraryRange.TypeConstraint == LibraryDependencyTarget.Package)
				.Select(l => new BuildIntegratedPackageReference(l, targetFramework));
		}

		public override async Task<bool> InstallPackageAsync(
			string packageId,
			VersionRange range,
			INuGetProjectContext nuGetProjectContext,
			BuildIntegratedInstallationContext installationContext,
			CancellationToken token)
		{
			// Right now, the UI only handles installation of specific versions, which is just the minimum version of
			// the provided version range.
			var formattedRange = range.MinVersion.ToNormalizedString();

			nuGetProjectContext.Log(MessageLevel.Info, "Installing package {0}.{1}", $"{packageId} {formattedRange}");

			if (installationContext.SuccessfulFrameworks.Any() && installationContext.UnsuccessfulFrameworks.Any())
			{
				// This is the "partial install" case. That is, install the package to only a subset of the frameworks
				// supported by this project.
				var conditionalService = _unconfiguredProject
					.Services
					.ExportProvider
					.GetExportedValue<IConditionalPackageReferencesService>();

				if (conditionalService == null)
				{
					throw new InvalidOperationException(string.Format("UnableToGetCPSPackageInstallationService {0}", _projectFullPath));
				}

				foreach (var framework in installationContext.SuccessfulFrameworks)
				{
					if (!installationContext.OriginalFrameworks.TryGetValue(framework, out string originalFramework))
					{
						originalFramework = framework.GetShortFolderName();
					}

					var reference = await conditionalService.AddAsync(
						packageId,
						formattedRange,
						TargetFrameworkCondition,
						originalFramework);

					// SuppressParent could be set to All if developmentDependency flag is true in package nuspec file.
					if (installationContext.SuppressParent != LibraryIncludeFlagUtils.DefaultSuppressParent &&
						installationContext.IncludeType != LibraryIncludeFlags.All)
					{
						await SetPackagePropertyValueAsync(
							reference.Metadata,
							ProjectItemProperties.PrivateAssets,
							MSBuildStringUtility.Convert(LibraryIncludeFlagUtils.GetFlagString(installationContext.SuppressParent)));

						await SetPackagePropertyValueAsync(
							reference.Metadata,
							ProjectItemProperties.IncludeAssets,
							MSBuildStringUtility.Convert(LibraryIncludeFlagUtils.GetFlagString(installationContext.IncludeType)));
					}
				}
			}
			else
			{
				// Install the package to all frameworks.
				var configuredProject = await _unconfiguredProject.GetSuggestedConfiguredProjectAsync();

				var result = await configuredProject
					.Services
					.PackageReferences
					.AddAsync(packageId, formattedRange);

				// This is the update operation
				if (!result.Added)
				{
					var existingReference = result.Reference;
					await existingReference.Metadata.SetPropertyValueAsync("Version", formattedRange);
				}

				if (installationContext.SuppressParent != LibraryIncludeFlagUtils.DefaultSuppressParent &&
					installationContext.IncludeType != LibraryIncludeFlags.All)
				{
					await SetPackagePropertyValueAsync(
						result.Reference.Metadata,
						ProjectItemProperties.PrivateAssets,
						MSBuildStringUtility.Convert(LibraryIncludeFlagUtils.GetFlagString(installationContext.SuppressParent)));

					await SetPackagePropertyValueAsync(
						result.Reference.Metadata,
						ProjectItemProperties.IncludeAssets,
						MSBuildStringUtility.Convert(LibraryIncludeFlagUtils.GetFlagString(installationContext.IncludeType)));
				}
			}

			return true;
		}

		private async Task SetPackagePropertyValueAsync(IProjectProperties metadata, string propertyName, string propertyValue)
		{
			await metadata.SetPropertyValueAsync(
				propertyName,
				propertyValue);
		}

		public override async Task<bool> UninstallPackageAsync(PackageIdentity packageIdentity, INuGetProjectContext nuGetProjectContext, CancellationToken token)
		{
			var configuredProject = await _unconfiguredProject.GetSuggestedConfiguredProjectAsync();
			await configuredProject?.Services.PackageReferences.RemoveAsync(packageIdentity.Id);
			return true;
		}

		public override Task<string> GetCacheFilePathAsync()
		{
			var spec = GetPackageSpec();
			if (spec == null)
			{
				throw new InvalidOperationException(string.Format("ProjectNotLoaded_RestoreFailed {0}", ProjectName));
			}

			return Task.FromResult(NoOpRestoreUtilities.GetProjectCacheFilePath(cacheRoot: spec.RestoreMetadata.OutputPath));
		}

		#endregion
	}
}
