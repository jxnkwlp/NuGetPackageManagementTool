using NuGet.ProjectManagement;
using System;
using System.IO;

namespace NuGetPackageManagerUI.VisualStudio.ProjectSystem
{
	public class WindowsStoreProjectSystem : VsMSBuildProjectSystem
	{
		public WindowsStoreProjectSystem(MSProjectManager projectManager, INuGetProjectContext projectContext) : base(projectManager, projectContext)
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
