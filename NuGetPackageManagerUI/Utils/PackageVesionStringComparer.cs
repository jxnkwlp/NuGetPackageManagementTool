using NuGet.Versioning;
using System.Collections.Generic;

namespace NuGetPackageManagerUI.Utils
{
	public class PackageVesionStringComparer : IComparer<string>
	{
		public int Compare(string x, string y)
		{
			return NuGetVersion.Parse(x).CompareTo(NuGetVersion.Parse(y));
		}
	}
}
