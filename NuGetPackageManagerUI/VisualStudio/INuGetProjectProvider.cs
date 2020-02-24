using NuGet.ProjectManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.VisualStudio
{
	public interface INuGetProjectProvider
	{
		Task<NuGetProject> TryCreateNuGetProjectAsync(
		   MSProjectManager mSProjectManager,
		   ProjectProviderContext context,
		   bool forceProjectType);
	}

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
