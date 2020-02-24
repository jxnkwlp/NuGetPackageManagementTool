using NuGet.Commands;
using NuGet.Common;
using NuGet.Frameworks;
using NuGet.PackageManagement.Utility;
using NuGet.ProjectManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.VisualStudio
{
	public class VsMSBuildProjectSystem
	   : IMSBuildProjectSystem
	   , IProjectSystemService
	{
		private const string BinDir = "bin";
		private const string NuGetImportStamp = "NuGetPackageImportStamp";
		private const string TargetName = "EnsureNuGetPackageBuildImports";

		private NuGetFramework _targetFramework;


		public NuGetFramework TargetFramework
		{
			get
			{
				if (_targetFramework == null)
				{
					var frameworkStrings = MSBuildProjectFrameworkUtility.GetProjectFrameworkStrings(
						projectFilePath: ProjectFileFullPath,
						targetFrameworks: GetPropertyValue("TargetFrameworks"),
						targetFramework: GetPropertyValue("TargetFramework"),
						targetFrameworkMoniker: GetPropertyValue("TargetFrameworkMoniker"),
						targetPlatformIdentifier: GetPropertyValue("TargetPlatformIdentifier"),
						targetPlatformVersion: GetPropertyValue("TargetPlatformVersion"),
						targetPlatformMinVersion: GetPropertyValue("TargetPlatformMinVersion"));

					// Parse the framework of the project or return unsupported.
					var frameworks = MSBuildProjectFrameworkUtility.GetProjectFrameworks(frameworkStrings).ToArray();

					if (frameworks.Length > 0)
					{
						_targetFramework = frameworks[0];
					}
					else
					{
						_targetFramework = NuGetFramework.UnsupportedFramework;
					}
				}

				return _targetFramework;
			}
		}

		public string ProjectName { get; }

		public string ProjectUniqueName { get; }

		public string ProjectFullPath { get; }

		public string ProjectFileFullPath { get; }

		public INuGetProjectContext NuGetProjectContext { get; set; }

		public MSProjectManager MSBuildProjectManager { get; }

		public VsMSBuildProjectSystem(string projectFullPath, INuGetProjectContext projectContext) : this(new MSProjectManager(projectFullPath), projectContext)
		{

		}

		public VsMSBuildProjectSystem(MSProjectManager projectManager, INuGetProjectContext projectContext)
		{
			MSBuildProjectManager = projectManager;
			ProjectFileFullPath = projectManager.ProjectFullPath;
			ProjectFullPath = Path.GetDirectoryName(projectManager.ProjectFullPath);
			ProjectName = Path.GetFileNameWithoutExtension(projectManager.ProjectFullPath);
			ProjectUniqueName = projectManager.ProjectFullPath;
			NuGetProjectContext = projectContext;
		}


		public virtual void AddBindingRedirects()
		{
		}

		public virtual void AddExistingFile(string path)
		{
			var fullPath = Path.Combine(ProjectFullPath, path);

			if (!File.Exists(fullPath))
			{
				throw new ArgumentNullException("PathToExistingFileNotPresent");
			}

			// TODO 
			var resolveFileConflict = NuGetProjectContext.ResolveFileConflict("");
			if (resolveFileConflict == FileConflictAction.Overwrite || resolveFileConflict == FileConflictAction.OverwriteAll)
			{
				AddFileCore(path, null);
			}
			else
			{
				AddFileCore(path, null);
			}

		}

		public virtual void AddFile(string path, Stream stream)
		{
			AddFileCore(path, () =>
			{
				FileSystemUtility.AddFile(ProjectFullPath, path, stream, NuGetProjectContext);
			});
		}

		public virtual Task AddFrameworkReferenceAsync(string name, string packageId)
		{
			return Task.CompletedTask;
		}

		public virtual void AddImport(string targetFullPath, ImportLocation location)
		{
			if (targetFullPath == null)
			{
				throw new ArgumentNullException("targetFullPath");
			}

			MSBuildProjectManager.AddImport(targetFullPath, location == ImportLocation.Top ? true : false);
		}

		public virtual Task AddReferenceAsync(string referencePath)
		{
			string fullPath = PathUtility.GetAbsolutePath(ProjectFullPath, referencePath);
			string relativePath = PathUtility.GetRelativePath(ProjectFileFullPath, fullPath);

			string assemblyFileName = Path.GetFileNameWithoutExtension(fullPath);

			try
			{
				var assemblyName = AssemblyName.GetAssemblyName(fullPath);
				assemblyFileName = assemblyName.FullName;
			}
			catch
			{
				// no-op
			}

			MSBuildProjectManager.AddItem(
									"Reference",
									assemblyFileName,
									new KeyValuePair<string, string>[] {
										new KeyValuePair<string, string>("HintPath", relativePath),
										new KeyValuePair<string, string>("Private", "True")
									});

			return Task.CompletedTask;
		}

		public virtual Task BeginProcessingAsync()
		{
			return Task.CompletedTask;
		}

		public virtual void DeleteDirectory(string path, bool recursive)
		{
			FileSystemUtility.DeleteDirectory(path, recursive, NuGetProjectContext);
		}

		public virtual async Task EndProcessingAsync()
		{
			Save();

			await Task.Delay(500).ConfigureAwait(false);
		}

		public virtual bool FileExistsInProject(string path)
		{
			return MSBuildProjectManager.GetItems().Any(t => t.EvaluatedInclude.Equals(path, StringComparison.OrdinalIgnoreCase) && (string.IsNullOrEmpty(t.ItemType) || t.ItemType[0] != '_'));
		}

		public virtual IEnumerable<string> GetDirectories(string path)
		{
			path = Path.Combine(ProjectFullPath, path);
			return Directory.EnumerateDirectories(path);
		}

		public virtual IEnumerable<string> GetFiles(string path, string filter, bool recursive)
		{
			path = Path.Combine(ProjectFullPath, path);
			return Directory.EnumerateFiles(path, filter, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
		}

		public virtual IEnumerable<string> GetFullPaths(string fileName)
		{
			foreach (var projectItem in MSBuildProjectManager.GetItems())
			{
				var itemFileName = Path.GetFileName(projectItem.EvaluatedInclude);

				if (string.Equals(fileName, itemFileName, StringComparison.OrdinalIgnoreCase))
				{
					yield return Path.Combine(ProjectFullPath, projectItem.EvaluatedInclude);
				}
			}
		}

		public virtual dynamic GetPropertyValue(string propertyName)
		{
			return MSBuildProjectManager.GetPropertyValue(propertyName);
		}

		public virtual bool IsSupportedFile(string path)
		{
			return true;
		}

		public virtual Task<bool> ReferenceExistsAsync(string name)
		{
			name = Path.GetFileNameWithoutExtension(name);

			var exists = GetReference(name) != null;

			return Task.FromResult(exists);
		}

		public virtual void RegisterProcessedFiles(IEnumerable<string> files)
		{
		}

		public virtual void RemoveFile(string path)
		{
			if (!string.IsNullOrEmpty(path))
			{
				var files = MSBuildProjectManager.GetItemsByEvaluatedInclude(path);
				if (files != null)
				{
					MSBuildProjectManager.RemoveItems(files);
					NuGetProjectContext.Log(MessageLevel.Debug, "RemovedFileToProject", path, ProjectName);
				}

				FileSystemUtility.DeleteFileAndParentDirectoriesIfEmpty(ProjectFullPath, path, NuGetProjectContext);
			}
		}

		public virtual void RemoveImport(string targetFullPath)
		{
			if (targetFullPath == null)
			{
				throw new ArgumentNullException("targetFullPath");
			}

			MSBuildProjectManager.RemoveImport(targetFullPath);
		}

		public virtual Task RemoveReferenceAsync(string name)
		{
			name = Path.GetFileNameWithoutExtension(name);

			dynamic item = GetReference(name);
			if (item != null)
				MSBuildProjectManager.RemoveItem(item);

			return Task.CompletedTask;
		}

		public virtual string ResolvePath(string path)
		{
			return path;
		}



		protected virtual void Save()
		{
			MSBuildProjectManager.CommitChanges();
		}

		protected virtual void AddFileToProject(string path)
		{
			if (!ExcludeFile(path))
			{
				MSBuildProjectManager.AddItem("Content", path);

				NuGetProjectContext.Log(MessageLevel.Debug, "AddedFileToProject", path, ProjectName);
			}
		}

		protected virtual bool ExcludeFile(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return true;
			}
			return Path.GetDirectoryName(path).Equals("bin", StringComparison.OrdinalIgnoreCase);
		}

		protected virtual void AddFileCore(string path, Action addFileAction)
		{
			if (string.IsNullOrEmpty(path))
			{
				return;
			}

			bool fileExistsInProject = FileExistsInProject(path);

			string fileName = Path.GetFileName(path);

			string lockFileFullPath = PackagesConfigLockFileUtility.GetPackagesLockFilePath(ProjectFullPath, GetPropertyValue("NuGetLockFilePath")?.ToString(), ProjectName);

			if (File.Exists(Path.Combine(ProjectFullPath, path))
					&& !fileExistsInProject
					&& !fileName.Equals(Constants.PackageReferenceFile)
					&& !fileName.Equals("packages." + ProjectName + ".config")
					&& !fileName.Equals("web.config")
					&& !fileName.Equals("app.config")
					&& !fileName.Equals(Path.GetFileName(lockFileFullPath)))
			{
				NuGetProjectContext.Log(MessageLevel.Warning, "File already exists", path);
				return;
			}

			addFileAction?.Invoke();

			if (!fileExistsInProject)
			{
				AddFileToProject(path);
			}
		}

		protected virtual object GetReference(string name)
		{
			return MSBuildProjectManager.GetItems("Reference").FirstOrDefault(item => new AssemblyName(item.EvaluatedInclude).Name.Equals(name, StringComparison.OrdinalIgnoreCase));
		}

		public virtual async Task SaveProjectAsync(CancellationToken token)
		{
			Save();
			await Task.Delay(500);
		}
	}
}