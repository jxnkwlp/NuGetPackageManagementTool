using NuGet.Packaging.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.Services
{
	public interface IProjectService : ISingletonService
	{
		event Action<string, string> PackageInstalling;
		event Action<string, string> PackageInstalled;
		event Action<string> PackageUninstalling;
		event Action<string> PackageUninstalled;

		Task InstallPackagesAsync(IEnumerable<string> projects, IEnumerable<PackageIdentity> packages, InstallPackagesOptions options, bool shouldThrow, CancellationToken token);

		Task UpdatePackagesAsync(IEnumerable<string> projects, IEnumerable<PackageIdentity> packages, UpdatePackagesOptions options, bool shouldThrow, CancellationToken token);

		Task UninstallPackagesAsync(IEnumerable<string> projects, IEnumerable<string> packageIds, UninstallPackagesOptions options, bool shouldThrow, CancellationToken token);

	}
}
