using NuGet.ProjectManagement;
using NuGetPackageManagerUI.MsBuild;
using NuGetPackageManagerUI.Utils;
using NuGetPackageManagerUI.VisualStudio;
using NuGetPackageManagerUI.VisualStudio.ProjectSystem;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.Services.ProjectSystems
{
	public static class MSBuildNuGetProjectSystemFactory
	{
		private static readonly Dictionary<string, Func<IProjectAdapter, INuGetProjectContext, MSBuildProjectSystem>> _factories = new Dictionary<string, Func<IProjectAdapter, INuGetProjectContext, MSBuildProjectSystem>>(StringComparer.OrdinalIgnoreCase)
		{
			 { VsProjectTypes.WebApplicationProjectTypeGuid, (pm, context)=> new WebProjectSystem(pm,context) },
			 { VsProjectTypes.WindowsStoreProjectTypeGuid, (pm, context)=> new WindowsStoreProjectSystem(pm,context) },
			 { VsProjectTypes.WixProjectTypeGuid, (pm, context)=> new WixProjectSystem(pm,context) },
			 { VsProjectTypes.DeploymentProjectTypeGuid, (pm, context)=> new MSBuildProjectSystem(pm,context) },
		};

		public static async Task<MSBuildProjectSystem> CreateMSBuildNuGetProjectSystemAsync(IProjectAdapter projectAdapter, INuGetProjectContext nuGetProjectContext)
		{
			var projectTypeGuids = await projectAdapter.GetProjectTypeGuidsAsync();

			foreach (var typeGuid in projectTypeGuids)
			{
				if (_factories.TryGetValue(typeGuid, out var func))
				{
					return func.Invoke(projectAdapter, nuGetProjectContext);
				}
			}

			return new MSBuildProjectSystem(projectAdapter, nuGetProjectContext);
		}
	}
}
