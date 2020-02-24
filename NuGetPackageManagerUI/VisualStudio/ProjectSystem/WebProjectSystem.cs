using NuGet.ProjectManagement;
using System;
using System.IO;

namespace NuGetPackageManagerUI.VisualStudio.ProjectSystem
{
	public class WebProjectSystem : VsMSBuildProjectSystem
	{
		public WebProjectSystem(MSProjectManager projectManager, INuGetProjectContext projectContext) : base(projectManager, projectContext)
		{
		}

		public override bool IsSupportedFile(string path)
		{
			var fileName = Path.GetFileName(path);
			return !(fileName.StartsWith("app.", StringComparison.OrdinalIgnoreCase) &&
					 fileName.EndsWith(".config", StringComparison.OrdinalIgnoreCase));
		}
	}
}
