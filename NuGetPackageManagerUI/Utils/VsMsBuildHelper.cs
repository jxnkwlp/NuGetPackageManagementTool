using System;
using System.IO;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.Utils
{
	public static class VsMsBuildHelper
	{
		public static async Task<string> FindVsMsBulidLocationAsync()
		{
			string libsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ThirdLibs");

			try
			{
				ProcessResult runResult = await Cmder.RunAsync(Path.Combine(libsPath, "vswhere.exe"), libsPath, "-latest -requires Microsoft.Component.MSBuild -find MSBuild\\**\\Bin\\Microsoft.Build.dll");

				if (runResult.Success)
				{
					return Path.GetDirectoryName(runResult.Text);
				}
			}
			catch (Exception)
			{
			}

			return null;
		}
	}
}
