using NuGet.ProjectManagement;
using NuGetPackageManagerUI.MsBuild;
using NuGetPackageManagerUI.VisualStudio;
using System;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.Services.ProjectServices
{
	public class VsMSBuildProjectSystemServices : INuGetProjectServices, IProjectSystemCapabilities, IProjectBuildProperties
	{
		private readonly MSBuildProjectSystem _vsProjectSystem;
		private readonly IProjectAdapter _projectAdapter;

		public VsMSBuildProjectSystemServices(MSBuildProjectSystem vsProjectSystem, IProjectAdapter projectAdapter)
		{
			_vsProjectSystem = vsProjectSystem;
			_projectAdapter = projectAdapter;
		}

		public IProjectBuildProperties BuildProperties => this;

		public IProjectSystemCapabilities Capabilities => this;

		public IProjectSystemReferencesReader ReferencesReader { get; set; }

		public IProjectSystemReferencesService References => throw new NotSupportedException();

		public IProjectSystemService ProjectSystem => _vsProjectSystem;

		public IProjectScriptHostService ScriptService { get; }

		public bool SupportsPackageReferences => true;

		public T GetGlobalService<T>() where T : class
		{
			throw new NotImplementedException();
		}

		public string GetPropertyValue(string propertyName)
		{
			return _projectAdapter.BuildProperties.GetPropertyValue(propertyName);
		}

		public async Task<string> GetPropertyValueAsync(string propertyName)
		{
			return await _projectAdapter.BuildProperties.GetPropertyValueAsync(propertyName);
		}
	}
}
