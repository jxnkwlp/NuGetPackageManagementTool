using NuGet.ProjectManagement;
using System;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.VisualStudio
{
	public class VsMSBuildProjectSystemServices : INuGetProjectServices, IProjectSystemCapabilities, IProjectBuildProperties
	{
		private readonly VsMSBuildProjectSystem _vsProjectSystem;
		private readonly MSProjectManager _projectManager;

		public VsMSBuildProjectSystemServices(VsMSBuildProjectSystem vsProjectSystem, MSProjectManager projectManager)
		{
			_vsProjectSystem = vsProjectSystem;
			_projectManager = projectManager;
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
			return _projectManager.GetPropertyValue(propertyName);
		}

		public Task<string> GetPropertyValueAsync(string propertyName)
		{
			return Task.FromResult(GetPropertyValue(propertyName));
		}
	}
}
