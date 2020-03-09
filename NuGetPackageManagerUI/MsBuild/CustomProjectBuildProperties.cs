using NuGet.ProjectManagement;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.MsBuild
{
	public class CustomProjectBuildProperties : IProjectBuildProperties
	{
		private readonly MsBuildProject _project;

		public CustomProjectBuildProperties(MsBuildProject project)
		{
			_project = project;
		}

		public string GetPropertyValue(string propertyName)
		{
			return _project.GetPropertyValue(propertyName);
		}

		public Task<string> GetPropertyValueAsync(string propertyName)
		{
			return Task.FromResult(GetPropertyValue(propertyName));
		}
	}
}
