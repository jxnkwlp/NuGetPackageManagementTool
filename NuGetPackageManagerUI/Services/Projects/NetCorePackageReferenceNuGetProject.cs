using NuGet.Commands;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.LibraryModel;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.ProjectManagement.Projects;
using NuGet.ProjectModel;
using NuGet.Versioning;
using NuGetPackageManagerUI.MsBuild;
using NuGetPackageManagerUI.Services.ProjectServices;
using NuGetPackageManagerUI.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.Services.Projects
{
	/// <summary>
	///  .Net core project with package reference project
	/// </summary>
	/// <remarks>
	///	 Impl: <see cref="INuGetIntegratedProject"/>, <see cref="IDependencyGraphProject"/>
	/// </remarks>
	public class NetCorePackageReferenceNuGetProject : BuildIntegratedNuGetProject
	{
		private const string TargetFrameworkCondition = "TargetFramework";
		private readonly IProjectRestoreService _projectRestoreService;

		public override string ProjectName => ProjectAdapter.ProjectName;
		public override string MSBuildProjectPath => ProjectAdapter.ProjectFilePath;

		public IProjectAdapter ProjectAdapter { get; }


		public NetCorePackageReferenceNuGetProject(IProjectAdapter projectAdapter, INuGetProjectServices projectServices, IProjectRestoreService projectRestoreService)
		{
			ProjectStyle = ProjectStyle.PackageReference;

			ProjectAdapter = projectAdapter;
			_projectRestoreService = projectRestoreService;
			//ProjectServices = projectServices;

			InternalMetadata.Add(NuGetProjectMetadataKeys.Name, projectAdapter.ProjectName);
			InternalMetadata.Add(NuGetProjectMetadataKeys.UniqueName, projectAdapter.ProjectUniqueName);
			InternalMetadata.Add(NuGetProjectMetadataKeys.FullPath, projectAdapter.ProjectFilePath);
			InternalMetadata.Add(NuGetProjectMetadataKeys.ProjectId, projectAdapter.ProjectId);
		}

		public override async Task<string> GetAssetsFilePathAsync()
		{
			return await GetAssetsFilePathAsync(shouldThrow: true);
		}

		public override async Task<string> GetCacheFilePathAsync()
		{
			var spec = await GetPackageSpecAsync();
			if (spec == null)
			{
				throw new InvalidOperationException(string.Format("ProjectNotLoaded_RestoreFailed", ProjectName));
			}

			return NoOpRestoreUtilities.GetProjectCacheFilePath(cacheRoot: spec.RestoreMetadata.OutputPath);
		}

		public override async Task<string> GetAssetsFilePathOrNullAsync()
		{
			return await GetAssetsFilePathAsync(shouldThrow: false);
		}

		public override Task AddFileToProjectAsync(string filePath)
		{
			// NO-OP
			return Task.CompletedTask;
		}

		public override async Task<IReadOnlyList<PackageSpec>> GetPackageSpecsAsync(DependencyGraphCacheContext context)
		{
			var projects = new List<PackageSpec>();

			DependencyGraphSpec projectRestoreInfo = await _projectRestoreService.GetOrCreateDependencyGraphSpecAsync(ProjectAdapter, this);
			if (projectRestoreInfo == null)
			{
				throw new InvalidOperationException(string.Format("ProjectNotLoaded_RestoreFailed", ProjectName));
			}

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

			return projects;
		}

		public override Task<bool> InstallPackageAsync(string packageId, VersionRange range, INuGetProjectContext nuGetProjectContext, BuildIntegratedInstallationContext installationContext, CancellationToken token)
		{
			var formattedRange = range.MinVersion.ToNormalizedString();

			nuGetProjectContext.Log(MessageLevel.Info, "Installing {0}", $"{packageId} {formattedRange}");

			if (installationContext.SuccessfulFrameworks.Any() && installationContext.UnsuccessfulFrameworks.Any())
			{
				foreach (var framework in installationContext.SuccessfulFrameworks)
				{
					if (!installationContext.OriginalFrameworks.TryGetValue(framework, out string originalFramework))
					{
						originalFramework = framework.GetShortFolderName();
					}

					var metadata = new Dictionary<string, string>();

					// SuppressParent could be set to All if developmentDependency flag is true in package nuspec file.
					if (installationContext.SuppressParent != LibraryIncludeFlagUtils.DefaultSuppressParent &&
						installationContext.IncludeType != LibraryIncludeFlags.All)
					{
						metadata = new Dictionary<string, string> {
								{ ProjectItemProperties.PrivateAssets,MSBuildStringUtility.Convert(LibraryIncludeFlagUtils.GetFlagString(installationContext.SuppressParent))},
								{ ProjectItemProperties.IncludeAssets, MSBuildStringUtility.Convert(LibraryIncludeFlagUtils.GetFlagString(installationContext.IncludeType))}
							   };
					}

					AddOrUpdatePackageReference(packageId, formattedRange, metadata, TargetFrameworkCondition, originalFramework);

				}
			}
			else
			{
				// Install the package to all frameworks.  

				var metadata = new Dictionary<string, string>();

				if (installationContext.SuppressParent != LibraryIncludeFlagUtils.DefaultSuppressParent &&
					installationContext.IncludeType != LibraryIncludeFlags.All)
				{
					metadata.Add(ProjectItemProperties.PrivateAssets, MSBuildStringUtility.Convert(LibraryIncludeFlagUtils.GetFlagString(installationContext.SuppressParent)));
					metadata.Add(ProjectItemProperties.IncludeAssets, MSBuildStringUtility.Convert(LibraryIncludeFlagUtils.GetFlagString(installationContext.IncludeType)));
				}

				AddOrUpdatePackageReference(packageId, formattedRange, metadata, null, null);

			}

			return Task.FromResult(true);
		}

		public override Task<bool> UninstallPackageAsync(PackageIdentity packageIdentity, INuGetProjectContext nuGetProjectContext, CancellationToken token)
		{
			RemovePackageReference(packageIdentity.Id, null, null);

			return Task.FromResult(true);
		}

		public override async Task<IEnumerable<PackageReference>> GetInstalledPackagesAsync(CancellationToken token)
		{
			PackageReference[] installedPackages;

			var packageSpec = await GetPackageSpecAsync();
			if (packageSpec != null)
			{
				installedPackages = GetPackageReferences(packageSpec);
			}
			else
			{
				installedPackages = new PackageReference[0];
			}

			return installedPackages;
		}


		private async Task<string> GetAssetsFilePathAsync(bool shouldThrow)
		{
			var packageSpec = await GetPackageSpecAsync();
			if (packageSpec == null)
			{
				if (shouldThrow)
				{
					throw new InvalidOperationException(string.Format("ProjectNotLoaded_RestoreFailed", ProjectName));
				}
				else
				{
					return null;
				}
			}

			return Path.Combine(
				packageSpec.RestoreMetadata.OutputPath,
				LockFileFormat.AssetsFileName);
		}

		private async Task<PackageSpec> GetPackageSpecAsync()
		{
			var dg = await _projectRestoreService.GetOrCreateDependencyGraphSpecAsync(ProjectAdapter, this);
			return dg.GetProjectSpec(ProjectAdapter.ProjectUniqueName);
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


		/// <summary>
		/// Creates an package reference and adds it to the project, or returns the existing reference.
		/// If added, the package reference will be added to the first item group conditioned as Condition="'$(conditionKey)' == 'conditionValue'"
		/// If there is no item group with such condition, then a new item group with that condition will be created and
		/// the new reference will be added to the newly created item group.
		/// </summary>
		private void AddOrUpdatePackageReference(string packageIdentity, string version, IEnumerable<KeyValuePair<string, string>> metadata, string conditionKey, string conditionValue)
		{
			// TODO conditionKey & conditionValue

			var project = ProjectAdapter.Project;

			var projectItem = project.GetItemsByEvaluatedInclude(packageIdentity)?.FirstOrDefault();
			if (projectItem != null)
			{
				var metadatas = project.GetMetadatas(projectItem);
				if (metadatas != null)
					foreach (var metadata1 in metadatas)
					{
						project.RemoveMetadata(projectItem, metadata1.Name);
					}

				foreach (var item in metadata)
				{
					project.AddItemMetadata(projectItem, item.Key, item.Value, false);
				}

				project.AddItemMetadata(projectItem, "Version", version, true);
			}
			else
			{
				project.AddItem("PackageReference", packageIdentity, metadata);
				projectItem = project.GetItemsByEvaluatedInclude(packageIdentity).FirstOrDefault();
				project.AddItemMetadata(projectItem, "Version", version, true);
			}

			project.CommitChanges();
		}

		/// <summary>
		/// Removes the package reference with the given identity from the package references.
		/// </summary>
		private void RemovePackageReference(string packageIdentity, string conditionKey, string conditionValue)
		{
			// TODO conditionKey & conditionValue

			var project = ProjectAdapter.Project;

			var items = project.GetItemsByEvaluatedInclude(packageIdentity);

			if (items != null)
			{
				project.RemoveItems(items);
				project.CommitChanges();
			}
		}

		private bool PackageReferenceExists(MsBuildProject project, string packageIdentity)
		{
			var items = project.GetItemsByEvaluatedInclude(packageIdentity);

			return items != null;
		}
	}
}
