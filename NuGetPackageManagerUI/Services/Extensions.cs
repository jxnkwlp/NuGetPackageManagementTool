using NuGet.Frameworks;
using NuGet.ProjectManagement;
using NuGetPackageManagerUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.Services
{
	public static class Extensions
	{
		public static ProjectModel ToProjectModel(this NuGetProject nuGetProject)
		{
			var fullPath = nuGetProject.GetMetadata<string>(NuGetProjectMetadataKeys.FullPath);
			var nuGetFrameworkName = ((NuGetFramework)nuGetProject.GetMetadataOrNull(NuGetProjectMetadataKeys.TargetFramework))?.GetShortFolderName();

			return new ProjectModel()
			{
				Name = nuGetProject.GetMetadata<string>(NuGetProjectMetadataKeys.Name),
				FullPath = fullPath,
				FolderPath = Path.GetDirectoryName(fullPath),
				FrameworkName = nuGetFrameworkName,
			};
		}
	}
}
