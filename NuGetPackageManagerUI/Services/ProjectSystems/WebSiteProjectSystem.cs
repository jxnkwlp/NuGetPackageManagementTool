//using NuGet.ProjectManagement;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Threading.Tasks;

//namespace NuGetPackageManagerUI.VisualStudio.ProjectSystem
//{
//	// TODO 
//	public class WebSiteProjectSystem : WebProjectSystem
//	{
//		private const string RootNamespace = "RootNamespace";
//		private const string AppCodeFolder = "App_Code";
//		private const string DefaultNamespace = "ASP";
//		private const string GeneratedFilesFolder = "Generated___Files";
//		private readonly HashSet<string> _excludedCodeFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

//		private static readonly string[] _sourceFileExtensions = { ".cs", ".vb" };

//		public WebSiteProjectSystem(MSBuildProjectManager projectManager, INuGetProjectContext projectContext) : base(projectManager, projectContext)
//		{
//		}

//		public override Task AddReferenceAsync(string referencePath)
//		{
//			var name = Path.GetFileNameWithoutExtension(referencePath);

//			return base.AddReferenceAsync(referencePath);
//		}
//	}
//}
