using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.Services
{
	public class SolutionDiretoryManager : ISolutionDiretoryManager
	{
		public static IEnumerable<string> DefaultProjectTypes => new[] { "csproj" };

		private IEnumerable<string> _projectFileCache;

		public string DiretoryPath { get; private set; }

		public SolutionDiretoryManager()
		{
		}

		public void OpenDiretory(string path)
		{
			DiretoryPath = path;
		}

		public Task<IEnumerable<string>> GetProjectFilesAsync(bool focus = false)
		{
			return Task.Run(() =>
			{
				if (!Directory.Exists(DiretoryPath)) return Enumerable.Empty<string>();

				if (_projectFileCache == null || focus)
				{
					var projectFiles = DefaultProjectTypes.SelectMany(t => Directory.GetFiles(DiretoryPath, $"*.{t}", SearchOption.AllDirectories));

					_projectFileCache = projectFiles.ToArray();
				}

				return _projectFileCache;
			});
		}

		//public async Task<IEnumerable<ProjectModel>> GetProjectsAsync(bool includePackages, bool focusLoad = false)
		//{
		//	if (string.IsNullOrEmpty(DiretoryPath))
		//		throw new Exception("DiretoryPath is null. You muse call 'OpenDiretory'.");

		//	if (_projectLoadCache == null || focusLoad)
		//	{
		//		if (includePackages)
		//		{
		//			_projectLoadCache = await SearchProjectsWithPackagesAsync(DiretoryPath, true, DefaultProjectTypes);
		//		}
		//		else
		//			_projectLoadCache = await SearchProjectsAsync(DiretoryPath, true, DefaultProjectTypes);
		//	}

		//	return _projectLoadCache;
		//}

		//private static async Task<IEnumerable<ProjectModel>> SearchProjectsWithPackagesAsync(string directory, bool subDirectories, IEnumerable<string> projectTypes)
		//{
		//	var projects = await SearchProjectsAsync(directory, subDirectories, projectTypes);

		//	return projects.Select(t => new ProjectModel()
		//	{
		//		FolderPath = t.FolderPath,
		//		Framework = t.Framework,
		//		FrameworkName = t.FrameworkName,
		//		FullPath = t.FullPath,
		//		Name = t.Name,

		//		Packages = GetProjectPackages(t.FullPath, true),
		//	}).ToArray();
		//}

		//private static Task<IEnumerable<ProjectModel>> SearchProjectsAsync(string directory, bool subDirectories, IEnumerable<string> projectTypes)
		//{
		//	if (projectTypes == null)
		//		throw new ArgumentNullException(nameof(projectTypes));

		//	if (string.IsNullOrWhiteSpace(directory))
		//	{
		//		throw new ArgumentNullException(nameof(directory));
		//	}

		//	return Task.Run(() =>
		//	{
		//		if (!Directory.Exists(directory)) return Enumerable.Empty<ProjectModel>();

		//		var projectFiles = projectTypes.SelectMany(t => Directory.GetFiles(directory, $"*.{t}", subDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));

		//		return projectFiles.Select(t =>
		//		{
		//			var model = new ProjectModel()
		//			{
		//				FolderPath = Path.GetDirectoryName(t),
		//				Name = Path.GetFileNameWithoutExtension(t),
		//				FullPath = t,
		//				Framework = GetProjectNuGetFramework(t),
		//			};
		//			model.FrameworkName = model.Framework?.GetShortFolderName();
		//			return model;
		//		}).ToArray();
		//	});
		//}

		//private static NuGetFramework GetProjectNuGetFramework(string fullPath)
		//{
		//	if (string.IsNullOrWhiteSpace(fullPath))
		//	{
		//		throw new ArgumentNullException(nameof(fullPath));
		//	}

		//	XDocument doc = XDocument.Load(fullPath);
		//	XNamespace ns = doc.Root.Name.Namespace;
		//	// for .net framework
		//	XElement element = doc.Root.Elements(XName.Get("PropertyGroup", ns.NamespaceName)).Elements(XName.Get("TargetFrameworkVersion", ns.NamespaceName)).FirstOrDefault();
		//	if (element != null)
		//	{
		//		return new NuGetFramework(".NETFramework", new Version(element.Value.Substring(1)));
		//	}

		//	// for net core
		//	element = doc.Root.Elements(XName.Get("PropertyGroup", ns.NamespaceName)).Elements(XName.Get("TargetFramework", ns.NamespaceName)).FirstOrDefault();
		//	if (element != null)
		//	{
		//		return new NuGetFramework(element.Value);
		//	}

		//	// TODO more frameworks
		//	//
		//	return NuGetFramework.AnyFramework;
		//}

		//private static IEnumerable<PackageModel> GetProjectPackages(string fullPath, bool isPackageConfigFile)
		//{
		//	if (!isPackageConfigFile)
		//	{
		//		// TODO 
		//		return Enumerable.Empty<PackageModel>();
		//	}

		//	string configFile = Path.Combine(Path.GetDirectoryName(fullPath), "packages.config");
		//	return GetProjectPackagesFromConfigFile(configFile);
		//}

		//private static IEnumerable<PackageInstalledModel> GetProjectPackagesFromConfigFile(string configFile)
		//{
		//	if (!File.Exists(configFile))
		//	{
		//		return Enumerable.Empty<PackageInstalledModel>();
		//	}

		//	XDocument doc = XDocument.Load(configFile);
		//	return (from t in doc.Root.Elements()
		//			select new PackageInstalledModel
		//			{
		//				Id = t.Attribute("id").Value,
		//				Version = t.Attribute("version").Value,
		//				TargetFramework = t.Attribute("targetFramework")?.Value
		//			} into t
		//			orderby t.Id
		//			select t).ToArray();
		//}

	}
}
