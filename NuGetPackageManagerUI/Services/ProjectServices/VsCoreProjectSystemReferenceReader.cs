using NuGet.Common;
using NuGet.Frameworks;
using NuGet.LibraryModel;
using NuGet.ProjectManagement;
using NuGet.ProjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.Services.NuGets.ProjectServices
{
	public class VsCoreProjectSystemReferenceReader : IProjectSystemReferencesReader
	{
		public Task<IEnumerable<LibraryDependency>> GetPackageReferencesAsync(NuGetFramework targetFramework, CancellationToken token)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<ProjectRestoreReference>> GetProjectReferencesAsync(ILogger logger, CancellationToken token)
		{
			throw new NotImplementedException();
		}
	}
}
