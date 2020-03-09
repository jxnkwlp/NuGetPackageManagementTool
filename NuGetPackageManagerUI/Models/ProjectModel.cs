using NuGet.Frameworks;
using NuGet.Packaging.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NuGetPackageManagerUI.Models
{
	[DebuggerDisplay("{ToString()}")]
	public class ProjectModel : IEquatable<ProjectModel>
	{
		public string Name { get; set; }

		public string FolderPath { get; set; }

		public string FullPath { get; set; }

		public string FrameworkName { get; set; }

		public IEnumerable<PackageModel> Packages { get; set; } = Enumerable.Empty<PackageModel>();

		public override string ToString()
		{
			return $"{Name}, [{FrameworkName}] [{FullPath}]";
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as ProjectModel);
		}

		public bool Equals(ProjectModel other)
		{
			return other != null &&
				   Name == other.Name &&
				   FolderPath == other.FolderPath &&
				   FullPath == other.FullPath;
		}

		public override int GetHashCode()
		{
			var hashCode = 629663567;
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FolderPath);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FullPath);
			return hashCode;
		}

		public static bool operator ==(ProjectModel left, ProjectModel right)
		{
			return EqualityComparer<ProjectModel>.Default.Equals(left, right);
		}

		public static bool operator !=(ProjectModel left, ProjectModel right)
		{
			return !(left == right);
		}
	}
}
