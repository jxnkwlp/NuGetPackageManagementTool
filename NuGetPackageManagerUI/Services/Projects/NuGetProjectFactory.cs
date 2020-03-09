using NuGet.Common;
using NuGet.ProjectManagement;
using NuGetPackageManagerUI.MsBuild;
using NuGetPackageManagerUI.Services.NuGets.ProjectServices;
using NuGetPackageManagerUI.Services.Projects;
using NuGetPackageManagerUI.Services.ProjectServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.Services.NuGets.Projects
{
	public sealed class NuGetProjectFactory : IScopedService
	{
		private readonly IEnumerable<INuGetProjectProvider> _providers;
		private readonly ILogger _logger;

		private readonly IProjectRestoreService _projectRestoreService;

		public NuGetProjectFactory(IEnumerable<INuGetProjectProvider> providers, ILogger logger, IProjectRestoreService projectRestoreService)
		{
			_providers = providers;
			_logger = logger;
			_projectRestoreService = projectRestoreService;
		}

		public async Task<NuGetProject> TryCreateNuGetProjectAsync(IProjectAdapter vsProjectAdapter, ProjectProviderContext context)
		{
			var exceptions = new List<Exception>();
			foreach (var provider in _providers)
			{
				try
				{
					var nuGetProject = await provider.TryCreateNuGetProjectAsync(
						vsProjectAdapter,
						context,
						_projectRestoreService,
						forceProjectType: false);

					if (nuGetProject != null)
					{
						return nuGetProject;
					}
				}
				catch (Exception e)
				{
					// Ignore failures. If this method returns null, the problem falls 
					// into one of the other NuGet project types.
					exceptions.Add(e);
				}
			}

			exceptions.ForEach(e => _logger.LogError(e.ToString()));

			return null;
		}
	}
}
