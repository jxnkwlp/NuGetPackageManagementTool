using NuGet.ProjectManagement;
using NuGetPackageManagerUI.VisualStudio.ProjectSystem;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.VisualStudio
{
	public static class MSBuildNuGetProjectSystemFactory
	{
		private static readonly Dictionary<string, Func<MSProjectManager, INuGetProjectContext, VsMSBuildProjectSystem>> _factories = new Dictionary<string, Func<MSProjectManager, INuGetProjectContext, VsMSBuildProjectSystem>>(StringComparer.OrdinalIgnoreCase)
		{
			 { VsProjectTypes.WebApplicationProjectTypeGuid, (pm, context)=> new WebProjectSystem(pm,context) },
			 { VsProjectTypes.WindowsStoreProjectTypeGuid, (pm, context)=> new WindowsStoreProjectSystem(pm,context) },
			 { VsProjectTypes.WixProjectTypeGuid, (pm, context)=> new WixProjectSystem(pm,context) },
			 { VsProjectTypes.DeploymentProjectTypeGuid, (pm, context)=> new VsMSBuildProjectSystem(pm,context) },
		};

		public static Task<VsMSBuildProjectSystem> CreateMSBuildNuGetProjectSystemAsync(MSProjectManager mSProjectManager, INuGetProjectContext nuGetProjectContext)
		{
			var projectTypeGuid = mSProjectManager.GetProjectTypeGuid();

			if (_factories.TryGetValue(projectTypeGuid, out var func))
			{
				return Task.FromResult(func.Invoke(mSProjectManager, nuGetProjectContext));
			}

			return Task.FromResult(new VsMSBuildProjectSystem(mSProjectManager, nuGetProjectContext));
		}
	}
}
