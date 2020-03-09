using NuGet.PackageManagement;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.Services.NuGets
{
	public class MyDeleteOnRestartManager : IDeleteOnRestartManager
	{
		// The file extension to add to the empty files which will be placed adjacent to partially uninstalled package
		// directories marking them for removal the next time the solution is opened.
		private const string DeletionMarkerSuffix = ".deleteme";
		private const string DeletionMarkerFilter = "*" + DeletionMarkerSuffix;

		private readonly ISolutionManager _solutionManager;
		private readonly INuGetSettingsAccessor _nuGetSettingsAccessor;
		private string _packagesFolderPath;

		private NuGet.Configuration.ISettings Settings => _nuGetSettingsAccessor.Settings.Value;

		public event EventHandler<PackagesMarkedForDeletionEventArgs> PackagesMarkedForDeletionFound;


		public string PackagesFolderPath
		{
			get
			{
				if (_solutionManager.SolutionDirectory != null)
				{
					_packagesFolderPath = PackagesFolderPathUtility.GetPackagesFolderPath(_solutionManager, Settings);
				}

				return _packagesFolderPath;
			}

			set
			{
				_packagesFolderPath = value;
			}
		}

		public MyDeleteOnRestartManager(ISolutionManager solutionManager, INuGetSettingsAccessor nuGetSettingsAccessor)
		{
			_solutionManager = solutionManager;
			_nuGetSettingsAccessor = nuGetSettingsAccessor;
		}

		public void CheckAndRaisePackageDirectoriesMarkedForDeletion()
		{
			var packages = GetPackageDirectoriesMarkedForDeletion();
			if (packages.Any() && PackagesMarkedForDeletionFound != null)
			{
				var eventArgs = new PackagesMarkedForDeletionEventArgs(packages);
				PackagesMarkedForDeletionFound(this, eventArgs);
			}
		}

		public Task DeleteMarkedPackageDirectoriesAsync(INuGetProjectContext projectContext)
		{
			//await TaskScheduler.Default;

			try
			{
				var packages = GetPackageDirectoriesMarkedForDeletion(); // returns empty if PackagesFolderPath is null. No need to check again.
				foreach (var package in packages)
				{
					try
					{
						FileSystemUtility.DeleteDirectorySafe(package, true, projectContext);
					}
					finally
					{
						if (!Directory.Exists(package))
						{
							var deleteMeFilePath = package.TrimEnd('\\') + DeletionMarkerSuffix;
							FileSystemUtility.DeleteFile(deleteMeFilePath, projectContext);
						}
						else
						{
							projectContext.Log(MessageLevel.Warning, "FailedToDeleteMarkedPackageDirectory", package);
						}
					}
				}
			}
			catch (Exception e)
			{
				projectContext.Log(MessageLevel.Warning, "FailedToDeleteMarkedPackageDirectories", e.Message);
			}

			return Task.CompletedTask;
		}

		public IReadOnlyList<string> GetPackageDirectoriesMarkedForDeletion()
		{
			// PackagesFolderPath reads the configs, reference the local variable to avoid reading the configs continously
			var packagesFolderPath = PackagesFolderPath;
			if (packagesFolderPath == null)
			{
				return Array.Empty<string>();
			}

			var candidates = FileSystemUtility
				.GetFiles(packagesFolderPath, path: "", filter: DeletionMarkerFilter, recursive: false)
				// strip the DeletionMarkerFilter at the end of the path to get the package path.
				.Select(path => Path.Combine(packagesFolderPath, Path.ChangeExtension(path, null)))
				.ToList();

			var filesWithoutFolders = candidates.Where(path => !Directory.Exists(path));
			foreach (var directory in filesWithoutFolders)
			{
				File.Delete(directory + DeletionMarkerSuffix);
			}

			return candidates.Where(path => Directory.Exists(path)).ToList();
		}

		public void MarkPackageDirectoryForDeletion(PackageIdentity package, string packageDirectory, INuGetProjectContext projectContext)
		{
			if (PackagesFolderPath == null)
			{
				return;
			}

			try
			{
				// Use the overload that doesn't take the context, so the .deleteme file doesn't get added
				// to source control. See https://github.com/NuGet/Home/issues/1720
				using (FileSystemUtility.CreateFile(packageDirectory + DeletionMarkerSuffix))
				{
				}
			}
			catch (Exception e)
			{
				projectContext.Log(MessageLevel.Warning, "FailedToMarkPackageDirectoryForDeletion", packageDirectory, e.Message);
			}
		}
	}
}
