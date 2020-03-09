using NuGet.Commands;
using NuGet.ProjectManagement.Projects;
using NuGet.ProjectModel;
using NuGetPackageManagerUI.MsBuild;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.Services.ProjectServices
{
	public class ProjectRestoreService : IProjectRestoreService
	{
		private readonly Dictionary<string, DependencyGraphSpec> _projectDgCache = new Dictionary<string, DependencyGraphSpec>();
		private readonly Dictionary<string, FileSystemWatcher> _fileSystemWatchers = new Dictionary<string, FileSystemWatcher>();


		private void InitFileWatcher(IProjectAdapter projectAdapter)
		{
			if (_fileSystemWatchers.ContainsKey(projectAdapter.ProjectFilePath))
				return;

			FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(projectAdapter.ProjectDirectory)
			{
				EnableRaisingEvents = true,
			};

			fileSystemWatcher.Changed += FileSystemWatcher_Changed;
		}

		private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
		{
			Debug.WriteLine(e.FullPath);

			var path = e.FullPath;

			// path is project file path
			if (_projectDgCache.ContainsKey(path))
			{
				Debug.WriteLine($"File has changed. the project dg cache is removed. the project file path is {path}");
				_projectDgCache.Remove(path);
			}
		}

		public async Task<DependencyGraphSpec> GetOrCreateDependencyGraphSpecAsync(IProjectAdapter projectAdapter, BuildIntegratedNuGetProject project)
		{
			string projectFilePath = projectAdapter.ProjectFilePath;

			if (_projectDgCache.TryGetValue(projectFilePath, out var dg))
			{
				return dg;
			}

			var projectFileName = Path.GetFileName(projectFilePath);
			var outputPath = projectAdapter.Project.GetMSBuildProjectExtensionsPath();
			var filePath = Path.Combine(outputPath, DependencyGraphSpec.GetDGSpecFileName(projectFileName));

			// always restore
			await RestoreAsync(project);

			InitFileWatcher(projectAdapter);

			dg = DependencyGraphSpec.Load(filePath);
			_projectDgCache[projectFilePath] = dg;

			return dg;
		}

		public async Task RestoreAsync(BuildIntegratedNuGetProject project)
		{
			try
			{
				var projectDirectory = Path.GetDirectoryName(project.MSBuildProjectPath);
				var result = await Cmder.RunAsync("dotnet", projectDirectory, "restore");
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
		}
	}
}
