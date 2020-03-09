using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.Services.NuGets
{
	public interface INuGetPackageService : IScopedService
	{
		event Action<string, string> PackageInstalled;
		event Action<string, string> PackageInstalling;
		event Action<string> PackageUninstalled;
		event Action<string> PackageUninstalling;

		string SolutionDirectory { get; }
		string PackagesFolderPath { get; }

		IEnumerable<SourceRepository> PrimarySourcesRepository { get; }
		IEnumerable<SourceRepository> SecondarySourcesRepository { get; }

		Task InitialAsync();

		Task<NuGetVersion> GetNuGetLatestVersionAsync(string packageId, bool includePrerelease, bool includeUnlisted, CancellationToken token);
		Task<NuGetVersion> GetNuGetLatestVersionAsync(string packageId, SourceCacheContext sourceCacheContext, bool includePrerelease, bool includeUnlisted, CancellationToken token);

		Task<IEnumerable<NuGetVersion>> GetNuGetVersionsAsync(string packageId, CancellationToken token);
		Task<IEnumerable<NuGetVersion>> GetNuGetVersionsAsync(string packageId, SourceCacheContext sourceCacheContext, CancellationToken token);

		Task InstallAsync(IEnumerable<NuGetProject> projects, IEnumerable<PackageIdentity> packages, bool includePrerelease, bool ignoreDependencies, bool shouldThrow, CancellationToken token);
		Task UninstallAsync(IEnumerable<NuGetProject> projects, IEnumerable<string> packages, bool removeDependencies, bool forceRemove, bool shouldThrow, CancellationToken token);
		Task UpdateAsync(IEnumerable<NuGetProject> projects, IEnumerable<PackageIdentity> packages, bool shouldThrow, CancellationToken token);

		Task<IEnumerable<IPackageSearchMetadata>> SearchPackagesAsync(string sourceRepository, string searchTerm, int skip, int take, bool includePrerelease, IEnumerable<string> supportedFrameworks, CancellationToken token);
	}
}