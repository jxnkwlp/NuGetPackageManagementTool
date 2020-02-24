using NuGetPackageManagerUI.Properties;
using System.Collections.Generic;
using System.Linq;

namespace NuGetPackageManagerUI.Utils
{
	public class StorageHelper
	{
		public static string SearchPath
		{
			get => Settings.Default.SearchPath;
			set
			{
				Settings.Default.SearchPath = value;
				TryAddSearchPath(value);
				Settings.Default.Save();
			}
		}

		public static string MsBuildDirectory
		{
			get => Settings.Default.MsBuildDirectory;
			set
			{
				Settings.Default.MsBuildDirectory = value;
				Settings.Default.Save();
			}
		}

		public static IEnumerable<string> GetSavedSearchPath()
		{
			if (Settings.Default.SearchPathList == null) return Enumerable.Empty<string>();
			return Settings.Default.SearchPathList.Cast<string>();
		}

		public static void TryAddSearchPath(string value)
		{
			if (!GetSavedSearchPath().Any(t => t.Equals(value)))
			{
				if (Settings.Default.SearchPathList == null)
					Settings.Default.SearchPathList = new System.Collections.Specialized.StringCollection();

				Settings.Default.SearchPathList.Add(value);
			}
		}
	}
}
