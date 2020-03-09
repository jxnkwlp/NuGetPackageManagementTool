using NuGet.ProjectManagement;

namespace NuGetPackageManagerUI.Services.NuGets
{
	public static class Extensions
	{
		public static string GetProjectDirectory(this NuGetProject nuGetProject)
		{
			return nuGetProject.GetMetadataOrNull("Directory")?.ToString();
		}

		public static string GetProjectName(this NuGetProject nuGetProject)
		{
			return nuGetProject.GetMetadataOrNull(NuGetProjectMetadataKeys.Name)?.ToString();
		}

		public static string GetProjectFilePath(this NuGetProject nuGetProject)
		{
			return nuGetProject.GetMetadataOrNull(NuGetProjectMetadataKeys.FullPath)?.ToString();
		}
 
	}
}
