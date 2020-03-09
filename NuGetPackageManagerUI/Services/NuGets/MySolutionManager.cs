using NuGet.Common;
using NuGet.Configuration;
using NuGet.PackageManagement;
using NuGet.ProjectManagement;
using NuGetPackageManagerUI.MsBuild;
using NuGetPackageManagerUI.Services.NuGets.Projects;
using NuGetPackageManagerUI.Services.NuGets.ProjectServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.Services.NuGets
{
	internal class MySolutionManager : ISolutionManager
	{
		private static readonly Dictionary<string, NuGetProject> _projectCache = new Dictionary<string, NuGetProject>();

		private static readonly INuGetProjectContext EmptyNuGetProjectContext = new EmptyNuGetProjectContext();

		//private bool _initialized;
		private string _solutionDirectory;
		private string _packagesFolderPath;

		private readonly ILogger _logger;
		private readonly NuGetProjectFactory _nuGetProjectFactory;
		private readonly IProjectAdapterProvider _projectAdapterProvider;
		private readonly INuGetSettingsAccessor _nuGetSettingsAccessor;
		private readonly ISolutionDiretoryManager _solutionDiretoryManager;

		public ISettings NuGetSettings => _nuGetSettingsAccessor.Settings.Value;

		public IEnumerable<NuGetProject> NuGetProjects { get; private set; }

		public string PackagesFolderPath
		{
			get
			{
				return _packagesFolderPath;
			}
		}

		public string MsbuildDirectory { get; private set; }

		public string SolutionDirectory
		{
			get
			{
				return _solutionDirectory;
			}
		}

		public bool IsSolutionOpen => true;

		public INuGetProjectContext NuGetProjectContext { get; set; }

		#region event

		public event EventHandler SolutionOpening;

		public event EventHandler SolutionOpened;

		public event EventHandler SolutionClosing;

		public event EventHandler SolutionClosed;

		public event EventHandler<NuGetEventArgs<string>> AfterNuGetCacheUpdated;

		public event EventHandler<NuGetProjectEventArgs> NuGetProjectAdded;

		public event EventHandler<NuGetProjectEventArgs> NuGetProjectRemoved;

		public event EventHandler<NuGetProjectEventArgs> NuGetProjectRenamed;

		public event EventHandler<NuGetProjectEventArgs> NuGetProjectUpdated;

		public event EventHandler<NuGetProjectEventArgs> AfterNuGetProjectRenamed;

		public event EventHandler<ActionsExecutedEventArgs> ActionsExecuted;

		#endregion event

		public MySolutionManager(NuGetProjectFactory nuGetProjectFactory, IProjectAdapterProvider projectAdapterProvider, INuGetSettingsAccessor nuGetSettingsAccessor, ILogger logger, ISolutionDiretoryManager solutionDiretoryManager)
		{
			_logger = logger;
			_nuGetProjectFactory = nuGetProjectFactory;
			_projectAdapterProvider = projectAdapterProvider;
			_nuGetSettingsAccessor = nuGetSettingsAccessor;
			_solutionDiretoryManager = solutionDiretoryManager;

			InitializeParams();
		}

		public void InitializeParams()
		{
			_solutionDirectory = _solutionDiretoryManager.DiretoryPath;

			_packagesFolderPath = PackagesFolderPathUtility.GetPackagesFolderPath(this, NuGetSettings);
		}

		public Task<bool> DoesNuGetSupportsAnyProjectAsync()
		{
			return Task.FromResult(result: true);
		}

		public void EnsureSolutionIsLoaded()
		{
		}

		public async Task<NuGetProject> GetNuGetProjectAsync(string nuGetProjectSafeName)
		{
			var project = await GetOrCreateNuGetProjectAsync(nuGetProjectSafeName);

			return project;
		}

		public async Task<string> GetNuGetProjectSafeNameAsync(NuGetProject nuGetProject)
		{
			string name = nuGetProject.GetMetadata<string>(NuGetProjectMetadataKeys.Name);
			if (await GetNuGetProjectAsync(name) == nuGetProject)
			{
				return name;
			}
			return NuGetProject.GetUniqueNameOrName(nuGetProject);
		}

		public Task<IEnumerable<NuGetProject>> GetNuGetProjectsAsync()
		{
			return Task.FromResult(_projectCache.Select(t => t.Value));
		}

		public Task<bool> IsSolutionAvailableAsync()
		{
			return Task.FromResult(result: true);
		}

		public void OnActionsExecuted(IEnumerable<ResolvedAction> actions)
		{
			ActionsExecuted?.Invoke(this, new ActionsExecutedEventArgs(actions));
		}

		protected async Task<NuGetProject> CreateProjectAsync(IProjectAdapter projectAdapter, INuGetProjectContext nuGetProjectContext = null)
		{
			var context = new ProjectProviderContext(
			   nuGetProjectContext ?? EmptyNuGetProjectContext,
			   () => PackagesFolderPathUtility.GetPackagesFolderPath(this, NuGetSettings));

			return await _nuGetProjectFactory.TryCreateNuGetProjectAsync(projectAdapter, context);
		}

		private async Task<NuGetProject> GetOrCreateNuGetProjectAsync(string projectFilePath)
		{
			if (_projectCache.TryGetValue(projectFilePath, out var nuGetProject))
			{
				return nuGetProject;
			}

			var adaper = await _projectAdapterProvider.GetOrCreateAdapterAsync(projectFilePath, this);

			nuGetProject = await CreateProjectAsync(adaper);

			_projectCache[projectFilePath] = nuGetProject;

			return nuGetProject;
		}
	}
}
