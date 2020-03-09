using NuGet.Frameworks;
using NuGet.PackageManagement;
using NuGet.ProjectManagement;
using NuGet.RuntimeModel;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.MsBuild
{
	public interface IProjectAdapter
	{
		MsBuildProject Project { get; }
		IProjectBuildProperties BuildProperties { get; }
		ISolutionManager SolutionManager { get; set; }

		string ProjectId { get; }
		string ProjectName { get; }
		string ProjectUniqueName { get; }

		string ProjectFilePath { get; }
		string ProjectDirectory { get; }

		string SolutionDirectory { get; }

		bool IsSupported { get; }
		string PackageTargetFallback { get; }

		Task InitialAsync();
		Task<string[]> GetProjectTypeGuidsAsync();
		Task<FrameworkName> GetDotNetFrameworkNameAsync();
		Task<IEnumerable<string>> GetReferencedProjectsAsync();
		Task<IEnumerable<RuntimeDescription>> GetRuntimeIdentifiersAsync();
		Task<IEnumerable<CompatibilityProfile>> GetRuntimeSupportsAsync();
		Task<NuGetFramework> GetTargetFrameworkAsync();
		Task<string> GetRestorePackagesWithLockFileAsync();
		Task<string> GetNuGetLockFilePathAsync();
		Task<bool> IsRestoreLockedAsync();
	}
}
