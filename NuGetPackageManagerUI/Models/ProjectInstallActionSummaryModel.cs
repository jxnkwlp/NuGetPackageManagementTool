using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.Models
{
	public class ProjectInstallActionSummaryModel
	{
		public NuGetProject Project { get; }

		public IEnumerable<PackageReference> Installed { get; private set; }

		public IEnumerable<PackageIdentity> NeedInstall { get; private set; }

		public IEnumerable<PackageIdentity> NeedUpdate { get; private set; }

		public ProjectInstallActionSummaryModel(NuGetProject project)
		{
			Project = project;
		}

		public async Task<IEnumerable<PackageReference>> LoadInstalledPackagesAsync(CancellationToken token)
		{
			Installed = await Project.GetInstalledPackagesAsync(token);
			return Installed;
		}

		public void UpdateFromPackageList(IEnumerable<PackageIdentity> packages)
		{
			if (Installed == null || !Installed.Any())
			{
				NeedInstall = packages;
				return;
			}

			List<PackageIdentity> installList = new List<PackageIdentity>();
			List<PackageIdentity> updateList = new List<PackageIdentity>();

			foreach (PackageIdentity item in packages)
			{
				var packageId = item.Id;
				var version = item.Version;

				// not exists.
				if (!Installed.Any(t => t.PackageIdentity.Id.Equals(packageId, System.StringComparison.InvariantCultureIgnoreCase)))
				{
					installList.Add(item);
				}
				// version not equals
				else if (Installed.Any(t => t.PackageIdentity.Id.Equals(packageId, System.StringComparison.InvariantCultureIgnoreCase) && t.PackageIdentity.Version != version))
				{
					installList.Add(item);
					updateList.Add(item);
				}
			}

			NeedInstall = installList;
			NeedUpdate = updateList;
		}

	}
}
