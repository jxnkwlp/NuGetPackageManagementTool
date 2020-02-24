//using NuGetPackageManagerUI.Models;
//using NuGetPackageManagerUI.NuGet;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;

//namespace NuGetPackageManagerUI.Services
//{
//	public class UiService : IUiService
//	{
//		private readonly NuGetPackageService _nuGetPackageService;

//		public UiService(NuGetPackageService nuGetPackageService)
//		{
//			_nuGetPackageService = nuGetPackageService;
//		}

//		public Task InstallPackagesAsync(IEnumerable<ProjectModel> projects, IEnumerable<PackageModel> packages, InstallPackagesOptions options, CancellationToken token)
//		{
//			return Task.Run(async () =>
//			{
//				await _nuGetPackageService.InstallAsync(projects, packages, options.IncludePrerelease, options.IgnoreDependencies, token);
//			});
//		}

//		public Task UninstallPackagesAsync(IEnumerable<ProjectModel> projects, IEnumerable<string> packageIds, UninstallPackagesOptions options, CancellationToken token)
//		{
//			return Task.Run(async () =>
//			{
//				await _nuGetPackageService.UninstallAsync(projects, packageIds, options.RemoveDependencies, options.FocusRemove, token);
//			});
//		}
//	}
//}
