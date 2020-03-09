using NuGet.PackageManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.MsBuild
{
	public class ProjectAdapterProvider : IProjectAdapterProvider
	{
		public Task<IProjectAdapter> GetOrCreateAdapterAsync(string projectFilePath, ISolutionManager solutionManager = null)
		{
			return GetOrCreateAdapterAsync(new MsBuildProject(projectFilePath), solutionManager);
		}

		public async Task<IProjectAdapter> GetOrCreateAdapterAsync(MsBuildProject msBuildProject, ISolutionManager solutionManager = null)
		{
			var adapter = new ProjectAdapter(msBuildProject, solutionManager);
			await adapter.InitialAsync();

			return adapter;
		}
	}
}
