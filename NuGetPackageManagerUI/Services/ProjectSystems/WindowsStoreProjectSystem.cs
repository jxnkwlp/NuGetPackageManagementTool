using NuGet.ProjectManagement;
using NuGetPackageManagerUI.MsBuild;
using System;
using System.IO;

namespace NuGetPackageManagerUI.VisualStudio.ProjectSystem
{
	public class WindowsStoreProjectSystem : MSBuildProjectSystem
	{
		public WindowsStoreProjectSystem(IProjectAdapter projectAdapter, INuGetProjectContext projectContext) : base(projectAdapter, projectContext)
		{
		}

		public override bool IsSupportedFile(string path)
		{
			string fileName = Path.GetFileName(path);
			if (fileName.Equals("app.config", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			return base.IsSupportedFile(path);
		}
	}
}
