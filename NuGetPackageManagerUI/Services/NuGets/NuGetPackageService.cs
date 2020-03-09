using NuGet.Common;
using NuGet.Configuration;
using NuGet.PackageManagement;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.PackageExtraction;
using NuGet.Packaging.Signing;
using NuGet.ProjectManagement;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Versioning;
using NuGetPackageManagerUI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.Services.NuGets
{
	public class NuGetPackageService : INuGetPackageService
	{
		private PackageExtractionContext _packageExtractionContext;

		private IPackageSourceProvider _packageSourceProvider;
		private ISourceRepositoryProvider _sourceRepositoryProvider;
		private NuGetPackageManager _packageManager;

		private readonly DependencyBehavior _dependencyBehavior = DependencyBehavior.Lowest;

		private readonly ILogger _logger;
		private readonly INuGetSettingsAccessor _nuGetSettingsAccessor;
		private readonly ISolutionDiretoryManager _solutionDiretoryManager;
		protected readonly ISolutionManagerProvider _solutionManagerProvider;

		public string SolutionDirectory { get; private set; }
		public ISettings Settings { get; private set; }

		public string PackagesFolderPath { get; private set; }
		public IEnumerable<SourceRepository> PrimarySourcesRepository => _sourceRepositoryProvider.GetRepositories();
		public IEnumerable<SourceRepository> SecondarySourcesRepository => _sourceRepositoryProvider.GetRepositories().Where(t => t.PackageSource.IsEnabled);


		public ISolutionManager SolutionManager => _solutionManagerProvider.CreateOrGetSolutionManager();

		public event Action<string, string> PackageInstalling;
		public event Action<string, string> PackageInstalled;
		public event Action<string> PackageUninstalling;
		public event Action<string> PackageUninstalled;


		public NuGetPackageService(ILogger logger, INuGetSettingsAccessor nuGetSettingsAccessor, ISolutionDiretoryManager solutionDiretoryManager, ISolutionManagerProvider solutionManagerProvider)
		{
			_logger = logger;
			_nuGetSettingsAccessor = nuGetSettingsAccessor;
			_solutionDiretoryManager = solutionDiretoryManager;
			_solutionManagerProvider = solutionManagerProvider;
		}

		public Task InitialAsync()
		{
			List<Lazy<INuGetResourceProvider>> _resourceProviders = new List<Lazy<INuGetResourceProvider>>();
			_resourceProviders.AddRange(FactoryExtensionsV3.GetCoreV3(Repository.Provider));

			SolutionDirectory = _solutionDiretoryManager.DiretoryPath;
			Settings = _nuGetSettingsAccessor.Settings.Value;

			PackagesFolderPath = PackagesFolderPathUtility.GetPackagesFolderPath(SolutionDirectory, Settings);

			_packageSourceProvider = new PackageSourceProvider(Settings);

			_sourceRepositoryProvider = new SourceRepositoryProvider(_packageSourceProvider, _resourceProviders);

			_packageExtractionContext = new PackageExtractionContext(
												PackageSaveMode.Defaultv2,
												PackageExtractionBehavior.XmlDocFileSaveMode,
												ClientPolicyContext.GetClientPolicy(Settings, _logger),
												_logger);

			_packageManager = CreatePackageManager();

			return Task.CompletedTask;
		}

		public SourceCacheContext CreateSourceCacheContext()
		{
			return new SourceCacheContext();
		}


		public async Task<NuGetVersion> GetNuGetLatestVersionAsync(string packageId, bool includePrerelease, bool includeUnlisted, CancellationToken token)
		{
			return await GetNuGetLatestVersionAsync(packageId, new SourceCacheContext(), includePrerelease, includeUnlisted, token);
		}

		public async Task<NuGetVersion> GetNuGetLatestVersionAsync(string packageId, SourceCacheContext sourceCacheContext, bool includePrerelease, bool includeUnlisted, CancellationToken token)
		{
			List<MetadataResource> metadataResources = new List<MetadataResource>();
			foreach (SourceRepository repo in _sourceRepositoryProvider.GetRepositories())
			{
				MetadataResource metadataResource = await repo.GetResourceAsync<MetadataResource>();
				if (metadataResource != null)
				{
					metadataResources.Add(metadataResource);
				}
			}

			List<Task<NuGetVersion>> tasks = new List<Task<NuGetVersion>>();

			using (sourceCacheContext)
			{
				foreach (MetadataResource resource in metadataResources)
				{
					tasks.Add(resource.GetLatestVersion(packageId, includePrerelease, includeUnlisted, sourceCacheContext, _logger, token));
				}

				var nugetVersions = await Task.WhenAll(tasks);

				return nugetVersions.Where(t => t != null).Distinct().OrderByDescending(t => t).FirstOrDefault();
			}
		}

		public async Task<IEnumerable<IPackageSearchMetadata>> SearchPackagesAsync(string sourceRepository, string searchTerm, int skip, int take, bool includePrerelease, IEnumerable<string> supportedFrameworks, CancellationToken token)
		{
			var repositoryProvider = _sourceRepositoryProvider.GetRepositories().FirstOrDefault(t => t.PackageSource.Name == sourceRepository);
			if (repositoryProvider == null)
				repositoryProvider = _sourceRepositoryProvider.GetRepositories().FirstOrDefault(t => t.PackageSource.IsEnabled);

			if (repositoryProvider == null)
				return Enumerable.Empty<IPackageSearchMetadata>();

			var resource = await repositoryProvider.GetResourceAsync<PackageSearchResource>();

			if (resource == null)
			{
				throw new Exception($"The repository '{repositoryProvider.PackageSource.Name}' unsupport package search.");
			}

			var searchFilter = new SearchFilter(includePrerelease)
			{
				SupportedFrameworks = supportedFrameworks,
				IncludeDelisted = false,
				OrderBy = SearchOrderBy.Id,
			};

			return await resource.SearchAsync(searchTerm,
									 searchFilter,
									 skip,
									 take,
									 _logger,
									 token);
		}

		public async Task<IEnumerable<NuGetVersion>> GetNuGetVersionsAsync(string packageId, SourceCacheContext sourceCacheContext, CancellationToken token)
		{
			List<MetadataResource> metadataResources = new List<MetadataResource>();
			foreach (SourceRepository repo in _sourceRepositoryProvider.GetRepositories())
			{
				MetadataResource metadataResource = await repo.GetResourceAsync<MetadataResource>();
				if (metadataResource != null)
				{
					metadataResources.Add(metadataResource);
				}
			}

			List<Task<IEnumerable<NuGetVersion>>> tasks = new List<Task<IEnumerable<NuGetVersion>>>();

			using (sourceCacheContext)
			{
				foreach (MetadataResource resource in metadataResources)
				{
					tasks.Add(resource.GetVersions(packageId, sourceCacheContext, _logger, token));
				}

				var nugetVersions = await Task.WhenAll(tasks.ToArray());

				return nugetVersions.SelectMany(t => t).Distinct().OrderByDescending(t => t).ToList();
			}
		}

		public async Task<IEnumerable<NuGetVersion>> GetNuGetVersionsAsync(string packageId, CancellationToken token)
		{
			return await GetNuGetVersionsAsync(packageId, new SourceCacheContext(), token);
		}

		public async Task InstallAsync(IEnumerable<NuGetProject> projects,
								 IEnumerable<PackageIdentity> packages,
								 bool includePrerelease,
								 bool ignoreDependencies,
								 bool shouldThrow,
								 CancellationToken token)
		{
			MyProjectContext projectContext = new MyProjectContext(FileConflictAction.OverwriteAll)
			{
				ActionType = NuGetActionType.Install,
				PackageExtractionContext = _packageExtractionContext
			};

			List<ProjectInstallActionSummaryModel> projectActionSummarys = new List<ProjectInstallActionSummaryModel>();

			foreach (NuGetProject item in projects)
			{
				ProjectInstallActionSummaryModel summary = new ProjectInstallActionSummaryModel(item);

				await summary.LoadInstalledPackagesAsync(token);
				summary.UpdateFromPackageList(packages);

				projectActionSummarys.Add(summary);
			}

			var installProjects = projectActionSummarys.Where(t => t.NeedInstall?.Any() == true);

			if (installProjects.Any())
			{
				await InstallPackagesInternalAsync(installProjects, projectContext, shouldThrow, token);
			}

		}

		public async Task UpdateAsync(IEnumerable<NuGetProject> projects,
							 IEnumerable<PackageIdentity> packages,
							 bool shouldThrow,
							 CancellationToken token)
		{
			MyProjectContext projectContext = new MyProjectContext(FileConflictAction.OverwriteAll)
			{
				ActionType = NuGetActionType.UpdateAll,
				PackageExtractionContext = _packageExtractionContext
			};

			List<ProjectInstallActionSummaryModel> projectActionSummarys = new List<ProjectInstallActionSummaryModel>();

			foreach (NuGetProject item in projects)
			{
				ProjectInstallActionSummaryModel summary = new ProjectInstallActionSummaryModel(item);

				await summary.LoadInstalledPackagesAsync(token);
				summary.UpdateFromPackageList(packages);

				projectActionSummarys.Add(summary);
			}

			var installProjects = projectActionSummarys.Where(t => t.NeedUpdate?.Any() == true);

			if (installProjects.Any())
			{
				await UpdatePackagesInternalAsync(installProjects, projectContext, shouldThrow, token);
			}

		}

		public async Task UninstallAsync(IEnumerable<NuGetProject> projects, IEnumerable<string> packages, bool removeDependencies, bool forceRemove, bool shouldThrow, CancellationToken token)
		{
			MyProjectContext projectContext = new MyProjectContext(FileConflictAction.OverwriteAll)
			{
				ActionType = NuGetActionType.Uninstall,
				PackageExtractionContext = _packageExtractionContext
			};

			// sort packages 
			// packages = SortPackagesWhenUninstallAsync(projects, packages);

			foreach (string packageId in packages)
			{
				var uninstallationContext = new UninstallationContext(removeDependencies, forceRemove);
				IReadOnlyList<ResolvedAction> actions = await GetActionsForUninstallAsync(
															uninstallationContext: uninstallationContext,
															targets: projects,
															packageId: packageId,
															projectContext: projectContext,
															token: token);
				if (actions.Count != 0)
				{
					PackageUninstalling?.Invoke(packageId);

					try
					{
						await ExecuteActionsAsync(actions, null, projectContext, NullSourceCacheContext.Instance, token);
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex);
						if (shouldThrow)
							throw;
					}
					finally
					{
						PackageUninstalled?.Invoke(packageId);
					}
				}
			}
		}


		private async Task InstallPackagesInternalAsync(
			IEnumerable<ProjectInstallActionSummaryModel> summaries,
			INuGetProjectContext projectContext,
			bool shouldThrow,
			CancellationToken token)
		{
			projectContext.ActionType = NuGetActionType.Install;
			projectContext.OperationId = Guid.NewGuid();

			List<NuGetProject> projects = summaries.Select(t => t.Project).ToList();
			List<PackageIdentity> packages = summaries.SelectMany(t => t.NeedInstall).Distinct().ToList();

			using (SourceCacheContext sourceCacheContext = new SourceCacheContext())
			{
				foreach (PackageIdentity packageIdentity in packages)
				{
					IReadOnlyList<ResolvedAction> actions = await GetActionsForInstallAsync(projects,
																			 packageIdentity,
																			 projectContext,
																			 sourceCacheContext,
																			 token);

					if (actions.Count != 0)
					{
						PackageInstalling?.Invoke(packageIdentity.Id, packageIdentity.Version.ToNormalizedString());

						try
						{
							await ExecuteActionsAsync(actions, packageIdentity, projectContext, sourceCacheContext, token);
						}
						catch (Exception ex)
						{
							Debug.WriteLine(ex);
							if (shouldThrow)
								throw;
						}
						finally
						{
							PackageInstalled?.Invoke(packageIdentity.Id, packageIdentity.Version.ToNormalizedString());
						}
					}
				}
			}
		}

		private async Task UpdatePackagesInternalAsync(
			IEnumerable<ProjectInstallActionSummaryModel> summaries,
			INuGetProjectContext projectContext,
			bool shouldThrow,
			CancellationToken token)
		{
			projectContext.ActionType = NuGetActionType.UpdateAll;
			projectContext.OperationId = Guid.NewGuid();

			List<NuGetProject> projects = summaries.Select(t => t.Project).ToList();
			List<PackageIdentity> packages = summaries.SelectMany(t => t.NeedInstall).Distinct().ToList();

			using (SourceCacheContext sourceCacheContext = new SourceCacheContext())
			{
				foreach (PackageIdentity packageIdentity in packages)
				{
					IReadOnlyList<ResolvedAction> actions = await GetActionsForUpdateAsync(projects,
																			 packageIdentity,
																			 projectContext,
																			 sourceCacheContext,
																			 token);

					if (actions.Count != 0)
					{
						PackageInstalling?.Invoke(packageIdentity.Id, packageIdentity.Version.ToNormalizedString());

						try
						{
							await ExecuteActionsAsync(actions, packageIdentity, projectContext, sourceCacheContext, token);
						}
						catch (PackageReferenceRollbackException rollbackException)
						{
							StringBuilder sb = new StringBuilder();
							sb.AppendLine(rollbackException.Message);

							if (rollbackException.LogMessages != null)
							{
								foreach (var item in rollbackException.LogMessages)
								{
									sb.AppendLine(item.FormatWithCode());
								}
							}

							throw new Exception(sb.ToString());
						}
						catch (Exception ex)
						{
							Debug.WriteLine(ex);
							if (shouldThrow)
								throw;
						}
						finally
						{
							PackageInstalled?.Invoke(packageIdentity.Id, packageIdentity.Version.ToNormalizedString());
						}
					}
				}
			}
		}

		private async Task ExecuteActionsAsync(IEnumerable<ResolvedAction> actions,
										 PackageIdentity packageIdentity,
										 INuGetProjectContext projectContext,
										 SourceCacheContext sourceCacheContext,
										 CancellationToken token)
		{
			NuGetProject[] nuGetProjects = actions.Select(action => action.Project).Distinct().ToArray();

			IEnumerable<NuGetProjectAction> nuGetActions = actions.Select(action => action.Action);

			if (packageIdentity != null)
			{
				NuGetPackageManager.SetDirectInstall(packageIdentity, projectContext);
			}

			await _packageManager.ExecuteNuGetProjectActionsAsync(nuGetProjects, nuGetActions, projectContext, sourceCacheContext, token);

			NuGetPackageManager.ClearDirectInstall(projectContext);
		}

		protected async Task<IReadOnlyList<ResolvedAction>> GetActionsForInstallAsync(IEnumerable<NuGetProject> targets,
			PackageIdentity packageIdentity,
			INuGetProjectContext projectContext,
			SourceCacheContext sourceCacheContext,
			CancellationToken token)
		{
			GatherCache gatherCache = new GatherCache();

			bool includePrerelease = packageIdentity.Version.IsPrerelease;

			List<ResolvedAction> results = new List<ResolvedAction>();

			ResolutionContext resolutionContext = new ResolutionContext(_dependencyBehavior,
				includePrerelease,
				includeUnlisted: true,
				VersionConstraints.None,
				gatherCache,
				sourceCacheContext);

			foreach (NuGetProject target in targets)
			{
				if (!((await target.GetInstalledPackagesAsync(token))?.Any(t => t.PackageIdentity.Id.Equals(packageIdentity.Id, StringComparison.InvariantCultureIgnoreCase) && t.PackageIdentity.Version == packageIdentity.Version) ?? false))
				{
					try
					{
						var actions = await _packageManager.PreviewInstallPackageAsync(target, packageIdentity, resolutionContext, projectContext, PrimarySourcesRepository, SecondarySourcesRepository, token);
						results.AddRange(actions.Select(a => new ResolvedAction(target, a)));
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex);
						throw;
					}
				}
			}

			return results;
		}

		protected async Task<IReadOnlyList<ResolvedAction>> GetActionsForUpdateAsync(IEnumerable<NuGetProject> targets,
			PackageIdentity packageIdentity,
			INuGetProjectContext projectContext,
			SourceCacheContext sourceCacheContext,
			CancellationToken token)
		{
			GatherCache gatherCache = new GatherCache();

			bool includePrerelease = packageIdentity.Version.IsPrerelease;

			List<ResolvedAction> results = new List<ResolvedAction>();

			ResolutionContext resolutionContext = new ResolutionContext(_dependencyBehavior,
				includePrerelease,
				includeUnlisted: true,
				VersionConstraints.None,
				gatherCache,
				sourceCacheContext);

			try
			{
				var actions = await _packageManager.PreviewUpdatePackagesAsync(packageIdentity, targets, resolutionContext, projectContext, PrimarySourcesRepository, SecondarySourcesRepository, token);

				results.AddRange(actions.Select(a => new ResolvedAction(a.Project, a)));
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				throw;
			}

			return results;
		}

		protected async Task<IReadOnlyList<ResolvedAction>> GetActionsForUninstallAsync(IEnumerable<NuGetProject> targets,
			string packageId,
			INuGetProjectContext projectContext,
			UninstallationContext uninstallationContext,
			CancellationToken token)
		{
			List<ResolvedAction> results = new List<ResolvedAction>();

			foreach (NuGetProject target in targets)
			{
				IEnumerable<PackageReference> oldInstalledPackages = await target.GetInstalledPackagesAsync(token);

				if (oldInstalledPackages != null && oldInstalledPackages.Any(t => t.PackageIdentity.Id.Equals(packageId, StringComparison.InvariantCultureIgnoreCase)))
				{
					try
					{
						var actions = await _packageManager.PreviewUninstallPackageAsync(target, packageId, uninstallationContext, projectContext, token);

						results.AddRange(actions.Select(a => new ResolvedAction(target, a)));
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex);
						throw;
					}
				}
			}

			return results;
		}



		//public async Task<DownloadResourceResult> GetDownloadResourceResultAsync(PackageIdentity package, PackageDownloadContext packageDownloadContext)
		//{
		//	return await PackageDownloader.GetDownloadResourceResultAsync(_sourceRepositoryProvider.GetRepositories(), package, packageDownloadContext, SettingsUtility.GetGlobalPackagesFolder(_nugetSettings), _logger, default(CancellationToken));
		//}


		protected NuGetPackageManager CreatePackageManager()
		{
			return new NuGetPackageManager(_sourceRepositoryProvider, Settings, SolutionManager, new EmptyDeleteOnRestartManager());
		}



		//public static NuGetProject CreateProject(ProjectModel project, INuGetProjectContext projectContext, string msbuildDirectory, string packagesFolderPath)
		//{
		//	if (string.IsNullOrWhiteSpace(msbuildDirectory))
		//		throw new ArgumentNullException(msbuildDirectory);

		//	MSBuildProjectSystem projectSystem = new MSBuildProjectSystem(msbuildDirectory, project.FullPath, projectContext);
		//	return new MSBuildNuGetProject(projectSystem, packagesFolderPath, project.FolderPath);
		//}

	}
}
