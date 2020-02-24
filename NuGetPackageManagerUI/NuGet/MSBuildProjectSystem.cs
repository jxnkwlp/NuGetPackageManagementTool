//using Microsoft.Build.Evaluation;
//using NuGet.Commands;
//using NuGet.Common;
//using NuGet.Frameworks;
//using NuGet.PackageManagement.Utility;
//using NuGet.ProjectManagement;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;

//namespace NuGetPackageManagerUI.NuGet
//{
//	[Obsolete]
//	public class MSBuildProjectSystem : IMSBuildProjectSystem
//	{
//		private const string BinDir = "bin";
//		private const string NuGetImportStamp = "NuGetPackageImportStamp";
//		private const string TargetName = "EnsureNuGetPackageBuildImports";

//		private NuGetFramework _targetFramework;


//		public NuGetFramework TargetFramework
//		{
//			get
//			{
//				if (_targetFramework == null)
//				{
//					var frameworkStrings = MSBuildProjectFrameworkUtility.GetProjectFrameworkStrings(
//						projectFilePath: ProjectFileFullPath,
//						targetFrameworks: GetPropertyValue("TargetFrameworks"),
//						targetFramework: GetPropertyValue("TargetFramework"),
//						targetFrameworkMoniker: GetPropertyValue("TargetFrameworkMoniker"),
//						targetPlatformIdentifier: GetPropertyValue("TargetPlatformIdentifier"),
//						targetPlatformVersion: GetPropertyValue("TargetPlatformVersion"),
//						targetPlatformMinVersion: GetPropertyValue("TargetPlatformMinVersion"));

//					// Parse the framework of the project or return unsupported.
//					var frameworks = MSBuildProjectFrameworkUtility.GetProjectFrameworks(frameworkStrings).ToArray();

//					if (frameworks.Length > 0)
//					{
//						_targetFramework = frameworks[0];
//					}
//					else
//					{
//						_targetFramework = NuGetFramework.UnsupportedFramework;
//					}
//				}

//				return _targetFramework;
//			}
//		}

//		public string ProjectName { get; }

//		public string ProjectUniqueName { get; }

//		public string ProjectFullPath { get; }

//		public string ProjectFileFullPath { get; }

//		public INuGetProjectContext NuGetProjectContext { get; set; }

//		public MSBuildProjectManager MSBuildProjectManager { get; }


//		public MSBuildProjectSystem(string projectFullPath, INuGetProjectContext projectContext)
//		{
//			ProjectFileFullPath = projectFullPath;
//			ProjectFullPath = Path.GetDirectoryName(projectFullPath);
//			ProjectName = Path.GetFileNameWithoutExtension(projectFullPath);
//			ProjectUniqueName = projectFullPath;
//			NuGetProjectContext = projectContext;

//			MSBuildProjectManager = new MSBuildProjectManager(projectFullPath);
//		}


//		public void AddBindingRedirects()
//		{
//		}

//		public void AddExistingFile(string path)
//		{
//			var fullPath = Path.Combine(ProjectFullPath, path);

//			if (!File.Exists(fullPath))
//			{
//				throw new ArgumentNullException("PathToExistingFileNotPresent");
//			}

//			// TODO 
//			var resolveFileConflict = NuGetProjectContext.ResolveFileConflict("");
//			if (resolveFileConflict == FileConflictAction.Overwrite || resolveFileConflict == FileConflictAction.OverwriteAll)
//			{
//				AddFileCore(path, null);
//			}
//			else
//			{
//				AddFileCore(path, null);
//			}

//		}

//		public void AddFile(string path, Stream stream)
//		{
//			AddFileCore(path, () =>
//			{
//				FileSystemUtility.AddFile(ProjectFullPath, path, stream, NuGetProjectContext);
//			});
//		}

//		public Task AddFrameworkReferenceAsync(string name, string packageId)
//		{
//			return Task.CompletedTask;
//		}

//		public void AddImport(string targetFullPath, ImportLocation location)
//		{
//			if (targetFullPath == null)
//			{
//				throw new ArgumentNullException("targetFullPath");
//			}

//			MSBuildProjectManager.AddImport(targetFullPath, location == ImportLocation.Top ? true : false);
//		}

//		public Task AddReferenceAsync(string referencePath)
//		{
//			string fullPath = PathUtility.GetAbsolutePath(ProjectFullPath, referencePath);
//			string relativePath = PathUtility.GetRelativePath(ProjectFileFullPath, fullPath);

//			string assemblyFileName = Path.GetFileNameWithoutExtension(fullPath);

//			try
//			{
//				var assemblyName = AssemblyName.GetAssemblyName(fullPath);
//				assemblyFileName = assemblyName.FullName;
//			}
//			catch
//			{
//				// no-op
//			}

//			MSBuildProjectManager.AddItem(
//									"Reference",
//									assemblyFileName,
//									new KeyValuePair<string, string>[] {
//										new KeyValuePair<string, string>("HintPath", relativePath),
//										new KeyValuePair<string, string>("Private", "True")
//									});

//			return Task.CompletedTask;
//		}

//		public Task BeginProcessingAsync()
//		{
//			return Task.CompletedTask;
//		}

//		public void DeleteDirectory(string path, bool recursive)
//		{
//			FileSystemUtility.DeleteDirectory(path, recursive, NuGetProjectContext);
//		}

//		public async Task EndProcessingAsync()
//		{
//			Save();

//			await Task.Delay(500).ConfigureAwait(false);
//		}

//		public bool FileExistsInProject(string path)
//		{
//			return MSBuildProjectManager.GetItems().Any(t => t.EvaluatedInclude.Equals(path, StringComparison.OrdinalIgnoreCase) && (string.IsNullOrEmpty(t.ItemType) || t.ItemType[0] != '_'));
//		}

//		public IEnumerable<string> GetDirectories(string path)
//		{
//			path = Path.Combine(ProjectFullPath, path);
//			return Directory.EnumerateDirectories(path);
//		}

//		public IEnumerable<string> GetFiles(string path, string filter, bool recursive)
//		{
//			path = Path.Combine(ProjectFullPath, path);
//			return Directory.EnumerateFiles(path, filter, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
//		}

//		public IEnumerable<string> GetFullPaths(string fileName)
//		{
//			foreach (var projectItem in MSBuildProjectManager.GetItems())
//			{
//				var itemFileName = Path.GetFileName(projectItem.EvaluatedInclude);

//				if (string.Equals(fileName, itemFileName, StringComparison.OrdinalIgnoreCase))
//				{
//					yield return Path.Combine(ProjectFullPath, projectItem.EvaluatedInclude);
//				}
//			}
//		}

//		public dynamic GetPropertyValue(string propertyName)
//		{
//			return MSBuildProjectManager.GetPropertyValue(propertyName);
//		}

//		public bool IsSupportedFile(string path)
//		{
//			return true;
//		}

//		public Task<bool> ReferenceExistsAsync(string name)
//		{
//			name = Path.GetFileNameWithoutExtension(name);

//			var exists = GetReference(name) != null;

//			return Task.FromResult(exists);
//		}

//		public void RegisterProcessedFiles(IEnumerable<string> files)
//		{
//		}

//		public void RemoveFile(string path)
//		{
//			if (!string.IsNullOrEmpty(path))
//			{
//				var files = MSBuildProjectManager.GetItemsByEvaluatedInclude(path);
//				if (files != null)
//				{
//					MSBuildProjectManager.RemoveItems(files);
//					NuGetProjectContext.Log(MessageLevel.Debug, "RemovedFileToProject", path, ProjectName);
//				}

//				//string folderPath = Path.GetDirectoryName(path);
//				//string fullPath = FileSystemUtility.GetFullPath(ProjectFullPath, path);
//				FileSystemUtility.DeleteFileAndParentDirectoriesIfEmpty(ProjectFullPath, path, NuGetProjectContext);
//			}
//		}

//		public void RemoveImport(string targetFullPath)
//		{
//			if (targetFullPath == null)
//			{
//				throw new ArgumentNullException("targetFullPath");
//			}

//			//string targetRelativePath = PathUtility.GetPathWithForwardSlashes(PathUtility.GetRelativePath(PathUtility.EnsureTrailingSlash(ProjectFullPath), targetFullPath));

//			MSBuildProjectManager.RemoveImport(targetFullPath);

//			//if (Project.Xml.Imports != null)
//			//{
//			//	dynamic importElement = null;
//			//	foreach (dynamic import in Project.Xml.Imports)
//			//	{
//			//		dynamic currentPath = PathUtility.GetPathWithForwardSlashes(import.Project);
//			//		if (StringComparer.OrdinalIgnoreCase.Equals(targetRelativePath, currentPath))
//			//		{
//			//			importElement = import;
//			//			break;
//			//		}
//			//	}
//			//	if (importElement != null)
//			//	{
//			//		importElement.Parent.RemoveChild(importElement);
//			//		RemoveEnsureImportedTarget(targetRelativePath);
//			//		Project.ReevaluateIfNecessary();
//			//	}
//			//}
//		}

//		public Task RemoveReferenceAsync(string name)
//		{
//			name = Path.GetFileNameWithoutExtension(name);

//			dynamic item = GetReference(name);
//			if (item != null)
//				MSBuildProjectManager.RemoveItem(item);

//			return Task.CompletedTask;
//		}

//		public string ResolvePath(string path)
//		{
//			return path;
//		}



//		protected void Save()
//		{
//			MSBuildProjectManager.CommitChanges();
//		}

//		protected virtual void AddFileToProject(string path)
//		{
//			if (!ExcludeFile(path))
//			{
//				MSBuildProjectManager.AddItem("Content", path);

//				NuGetProjectContext.Log(MessageLevel.Debug, "AddedFileToProject", path, ProjectName);
//			}
//		}

//		protected virtual bool ExcludeFile(string path)
//		{
//			if (string.IsNullOrEmpty(path))
//			{
//				return true;
//			}
//			return Path.GetDirectoryName(path).Equals("bin", StringComparison.OrdinalIgnoreCase);
//		}

//		protected virtual void AddFileCore(string path, Action addFileAction)
//		{
//			if (string.IsNullOrEmpty(path))
//			{
//				return;
//			}

//			bool fileExistsInProject = FileExistsInProject(path);

//			string fileName = Path.GetFileName(path);

//			string lockFileFullPath = PackagesConfigLockFileUtility.GetPackagesLockFilePath(ProjectFullPath, GetPropertyValue("NuGetLockFilePath")?.ToString(), ProjectName);

//			if (File.Exists(Path.Combine(ProjectFullPath, path))
//					&& !fileExistsInProject
//					&& !fileName.Equals(Constants.PackageReferenceFile)
//					&& !fileName.Equals("packages." + ProjectName + ".config")
//					&& !fileName.Equals("web.config")
//					&& !fileName.Equals("app.config")
//					&& !fileName.Equals(Path.GetFileName(lockFileFullPath)))
//			{
//				NuGetProjectContext.Log(MessageLevel.Warning, "File already exists", path);
//				return;
//			}

//			addFileAction?.Invoke();

//			if (!fileExistsInProject)
//			{
//				AddFileToProject(path);
//			}
//		}

//		protected object GetReference(string name)
//		{
//			return MSBuildProjectManager.GetItems("Reference").FirstOrDefault(item => new AssemblyName(item.EvaluatedInclude).Name.Equals(name, StringComparison.OrdinalIgnoreCase));
//		}
//	}
//}
