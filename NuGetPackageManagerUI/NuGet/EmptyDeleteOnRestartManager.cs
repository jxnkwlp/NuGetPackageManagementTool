using NuGet.PackageManagement;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.NuGet
{
	public class EmptyDeleteOnRestartManager : IDeleteOnRestartManager
	{
		public event EventHandler<PackagesMarkedForDeletionEventArgs> PackagesMarkedForDeletionFound;

		public void CheckAndRaisePackageDirectoriesMarkedForDeletion()
		{
		}

		public Task DeleteMarkedPackageDirectoriesAsync(INuGetProjectContext projectContext)
		{
			return Task.CompletedTask;
		}

		public IReadOnlyList<string> GetPackageDirectoriesMarkedForDeletion()
		{
			return new List<string>();
		}

		public void MarkPackageDirectoryForDeletion(PackageIdentity package, string packageDirectory, INuGetProjectContext projectContext)
		{
		}
	}
}
