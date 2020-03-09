using NuGet.ProjectManagement;

namespace NuGetPackageManagerUI.Services.NuGets.Projects
{
	/// <summary>
	///  .Net framwork with packages.config project
	/// </summary>
	/// <remarks>
	///  Impl <see cref="IDependencyGraphProject"/>
	/// </remarks>
	public class NetFrameworkNuGetProject : MSBuildNuGetProject
	{
		public NetFrameworkNuGetProject(
				IMSBuildProjectSystem msbuildNuGetProjectSystem,
				string folderNuGetProjectPath,
				string packagesConfigFolderPath,
				INuGetProjectServices nuGetProjectServices) : base(msbuildNuGetProjectSystem, folderNuGetProjectPath, packagesConfigFolderPath)
		{
			//ProjectServices = nuGetProjectServices;
		}
	}
}
