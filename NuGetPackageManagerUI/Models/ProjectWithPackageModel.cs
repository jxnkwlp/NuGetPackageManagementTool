using System.Collections.Generic;
using System.Linq;

namespace NuGetPackageManagerUI.Models
{
	public class ProjectWithPackageModel : ProjectModel
	{
		public IEnumerable<PackageModel> Packages { get; set; } = Enumerable.Empty<PackageModel>();
	}

}
