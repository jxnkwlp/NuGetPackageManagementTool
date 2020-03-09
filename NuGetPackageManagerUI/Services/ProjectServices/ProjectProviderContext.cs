using NuGet.ProjectManagement;
using NuGetPackageManagerUI.Services.Projects;
using System;

namespace NuGetPackageManagerUI.Services.NuGets.ProjectServices
{
	/// <summary>
	/// Additional data passed to <see cref="INuGetProjectProvider"/> method 
	/// to create project instance.
	/// </summary>
	public class ProjectProviderContext
	{
		public INuGetProjectContext ProjectContext { get; }

		public Func<string> PackagesPathFactory { get; }

		public ProjectProviderContext(
			INuGetProjectContext projectContext,
			Func<string> packagesPathFactory)
		{
			if (projectContext == null)
			{
				throw new ArgumentNullException(nameof(projectContext));
			}

			if (packagesPathFactory == null)
			{
				throw new ArgumentNullException(nameof(packagesPathFactory));
			}

			ProjectContext = projectContext;
			PackagesPathFactory = packagesPathFactory;
		}
	}
}
