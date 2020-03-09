using NuGet.PackageManagement;
using NuGetPackageManagerUI.MsBuild;
using NuGetPackageManagerUI.Services.NuGets.Projects;

namespace NuGetPackageManagerUI.Services.NuGets
{
	public class SolutionManagerProvider : ISolutionManagerProvider
	{
		private string _solutionDiretory = null;
		private ISolutionManager _solutionManager;

		private readonly ISolutionDiretoryManager _solutionDiretoryManager;
		private readonly NuGetProjectFactory _nuGetProjectFactory;
		private readonly IProjectAdapterProvider _projectAdapterProvider;
		private readonly INuGetSettingsAccessor _nuGetSettingsAccessor;
		private readonly NuGet.Common.ILogger _logger;

		public SolutionManagerProvider(ISolutionDiretoryManager solutionDiretoryManager, NuGetProjectFactory nuGetProjectFactory, IProjectAdapterProvider projectAdapterProvider, INuGetSettingsAccessor nuGetSettingsAccessor, NuGet.Common.ILogger logger)
		{
			_solutionDiretoryManager = solutionDiretoryManager;
			_nuGetProjectFactory = nuGetProjectFactory;
			_projectAdapterProvider = projectAdapterProvider;
			_nuGetSettingsAccessor = nuGetSettingsAccessor;
			_logger = logger;
		}

		public ISolutionManager CreateOrGetSolutionManager()
		{
			var newDirectory = _solutionDiretoryManager.DiretoryPath;

			if (newDirectory != _solutionDiretory || _solutionManager == null)
			{
				_solutionManager = new MySolutionManager(_nuGetProjectFactory, _projectAdapterProvider, _nuGetSettingsAccessor, _logger, _solutionDiretoryManager);
			}

			return _solutionManager;
		}
	}
}
