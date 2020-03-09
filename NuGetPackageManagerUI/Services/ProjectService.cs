using NuGet.PackageManagement;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGetPackageManagerUI.Services.NuGets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.Services
{
	public class ProjectService : IProjectService
	{
		private readonly INuGetPackageService _nuGetPackageService;
		private readonly ISolutionDiretoryManager _solutionDiretoryManager;
		private readonly ISolutionManagerProvider _solutionManagerProvider;

		public event Action<string, string> PackageInstalling;
		public event Action<string, string> PackageInstalled;
		public event Action<string> PackageUninstalling;
		public event Action<string> PackageUninstalled;

		public ISolutionManager SolutionManager => _solutionManagerProvider.CreateOrGetSolutionManager();

		public ProjectService(INuGetPackageService nuGetPackageService, ISolutionDiretoryManager solutionDiretoryManager, ISolutionManagerProvider solutionManagerProvider)
		{
			_nuGetPackageService = nuGetPackageService;
			_solutionDiretoryManager = solutionDiretoryManager;
			_solutionManagerProvider = solutionManagerProvider;

			InitialEvents();
		}

		private void InitialEvents()
		{
			if (_nuGetPackageService != null)
			{
				_nuGetPackageService.PackageInstalled -= NuGetPackageInstalled;
				_nuGetPackageService.PackageInstalling -= NuGetPackageInstalling;
				_nuGetPackageService.PackageUninstalled -= NuGetPackageUninstalled;
				_nuGetPackageService.PackageUninstalling -= NuGetPackageUninstalling;
			}

			_nuGetPackageService.PackageInstalled += NuGetPackageInstalled;
			_nuGetPackageService.PackageInstalling += NuGetPackageInstalling;
			_nuGetPackageService.PackageUninstalled += NuGetPackageUninstalled;
			_nuGetPackageService.PackageUninstalling += NuGetPackageUninstalling;
		}

		public async Task InstallPackagesAsync(IEnumerable<string> projects, IEnumerable<PackageIdentity> packages, InstallPackagesOptions options, bool shouldThrow, CancellationToken token)
		{
			var nuGetProjects = new List<NuGetProject>();
			foreach (var item in projects)
			{
				var nuGetProject = await SolutionManager.GetNuGetProjectAsync(item);

				nuGetProjects.Add(nuGetProject);
			}

			await _nuGetPackageService.InstallAsync(nuGetProjects, packages, options.IncludePrerelease, options.IgnoreDependencies, shouldThrow, token);
		}

		public async Task UpdatePackagesAsync(IEnumerable<string> projects, IEnumerable<PackageIdentity> packages, UpdatePackagesOptions options, bool shouldThrow, CancellationToken token)
		{
			var nuGetProjects = new List<NuGetProject>();
			foreach (var item in projects)
			{
				var nuGetProject = await SolutionManager.GetNuGetProjectAsync(item);

				nuGetProjects.Add(nuGetProject);
			}

			await _nuGetPackageService.UpdateAsync(nuGetProjects, packages, shouldThrow, token);
		}

		public async Task UninstallPackagesAsync(IEnumerable<string> projects, IEnumerable<string> packageIds, UninstallPackagesOptions options, bool shouldThrow, CancellationToken token)
		{
			var nuGetProjects = new List<NuGetProject>();
			foreach (var item in projects)
			{
				var nuGetProject = await SolutionManager.GetNuGetProjectAsync(item);

				nuGetProjects.Add(nuGetProject);
			}

			if (packageIds.Any(t => t.Equals("NETStandard.Library")) == true)
			{
				packageIds = packageIds.Where(t => !t.Equals("NETStandard.Library"));
			}

			await _nuGetPackageService.UninstallAsync(nuGetProjects, packageIds, options.RemoveDependencies, options.FocusRemove, shouldThrow, token);
		}

		private void NuGetPackageUninstalling(string packageId)
		{
			PackageUninstalling?.Invoke(packageId);
		}

		private void NuGetPackageUninstalled(string packageId)
		{
			PackageUninstalled?.Invoke(packageId);
		}

		private void NuGetPackageInstalling(string packageId, string version)
		{
			PackageInstalling?.Invoke(packageId, version);
		}

		private void NuGetPackageInstalled(string packageId, string version)
		{
			PackageInstalled?.Invoke(packageId, version);
		}


	}
}
