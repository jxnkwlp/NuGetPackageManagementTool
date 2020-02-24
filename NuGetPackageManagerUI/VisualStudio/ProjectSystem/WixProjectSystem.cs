using NuGet.ProjectManagement;
using System;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.VisualStudio.ProjectSystem
{
	public class WixProjectSystem : VsMSBuildProjectSystem
	{ 

		private const string RootNamespace = "RootNamespace";
		private const string OutputName = "OutputName";
		private const string DefaultNamespace = "WiX";

		public WixProjectSystem(MSProjectManager projectManager, INuGetProjectContext projectContext) : base(projectManager, projectContext)
		{
		}

		public override Task AddReferenceAsync(string referencePath)
		{
			// References aren't allowed for WiX projects
			return Task.CompletedTask;
		}

		protected override bool ExcludeFile(string path)
		{
			// Exclude nothing from WiX projects
			return false;
		}

		public override dynamic GetPropertyValue(string propertyName)
		{
			if (propertyName.Equals(RootNamespace, StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					return base.GetPropertyValue(OutputName);
				}
				catch
				{
					return DefaultNamespace;
				}
			}
			return base.GetPropertyValue(propertyName);
		}

		public override Task RemoveReferenceAsync(string name)
		{
			// References aren't allowed for WiX projects
			return Task.CompletedTask;
		}

		public override Task<bool> ReferenceExistsAsync(string name)
		{
			// References aren't allowed for WiX projects
			return Task.FromResult(true);
		}

		public override bool IsSupportedFile(string path)
		{
			return true;
		}
	}
}
