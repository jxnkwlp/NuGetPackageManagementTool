using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NuGetPackageManagerUI.Models
{
	[DebuggerDisplay("{ToString()}")]
	public class PackageInstalledModel : PackageModel, IEquatable<PackageInstalledModel>
	{
		public string TargetFramework { get; set; }

		public override bool Equals(object obj)
		{
			return Equals(obj as PackageInstalledModel);
		}

		public bool Equals(PackageInstalledModel other)
		{
			return other != null &&
				   Id == other.Id &&
				   Version == other.Version;
		}

		public override int GetHashCode()
		{
			var hashCode = -612338121;
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Id);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Version);
			return hashCode;
		}

		public override string ToString()
		{
			return $"{Id}.{Version}, {TargetFramework}";
		}

		public static bool operator ==(PackageInstalledModel left, PackageInstalledModel right)
		{
			return EqualityComparer<PackageInstalledModel>.Default.Equals(left, right);
		}

		public static bool operator !=(PackageInstalledModel left, PackageInstalledModel right)
		{
			return !(left == right);
		}
	}
}
