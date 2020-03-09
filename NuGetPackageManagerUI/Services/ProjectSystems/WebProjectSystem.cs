using NuGet.ProjectManagement;
using NuGetPackageManagerUI.MsBuild;
using System;
using System.IO;

namespace NuGetPackageManagerUI.VisualStudio.ProjectSystem
{
	public class WebProjectSystem : MSBuildProjectSystem
	{
		public WebProjectSystem(IProjectAdapter projectAdapter, INuGetProjectContext projectContext) : base(projectAdapter, projectContext)
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
