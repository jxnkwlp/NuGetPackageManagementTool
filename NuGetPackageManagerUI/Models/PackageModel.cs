using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NuGetPackageManagerUI.Models
{
	[DebuggerDisplay("{ToString()}")]
	public class PackageModel : IEquatable<PackageModel>
	{
		public string Id { get; set; }

		public string Version { get; set; }

		public override bool Equals(object obj)
		{
			return Equals(obj as PackageModel);
		}

		public bool Equals(PackageModel other)
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

		public static bool operator ==(PackageModel left, PackageModel right)
		{
			return EqualityComparer<PackageModel>.Default.Equals(left, right);
		}

		public static bool operator !=(PackageModel left, PackageModel right)
		{
			return !(left == right);
		}

		public override string ToString()
		{
			return $"{Id}.{Version}";
		}
	}

}
