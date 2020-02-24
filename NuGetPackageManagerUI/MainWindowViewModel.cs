using Microsoft.VisualStudio.Threading;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuGetPackageManagerUI.Models;
using NuGetPackageManagerUI.Services;
using NuGetPackageManagerUI.Utils;
using NuGetPackageManagerUI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NuGetPackageManagerUI
{
	public class MainWindowViewModel : BaseViewModel
	{
		private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
		private readonly CancellationTokenSource _checkAllPackageCancellationTokenSource = new CancellationTokenSource();
		private PackagePreInstallWindowViewModel _packagePreInstallWindowViewModel = null;


		private string _searchPath;
		private string _logMessage;

		public string SearchPath { get => _searchPath; set => Set(ref _searchPath, value); }
		public string LogMessage { get => _logMessage; set => Set(ref _logMessage, value); }
		public int PercentageValue { get => _percentageValue; set => Set(ref _percentageValue, value); }

		public ObservableRangeCollection<string> SavedSearchPath { get; set; } = new ObservableRangeCollection<string>();
		public ObservableRangeCollection<PackageViewModel> PackageList { get; set; } = new ObservableRangeCollection<PackageViewModel>();
		public ObservableRangeCollection<ProjectViewModel> ProjectList { get; set; } = new ObservableRangeCollection<ProjectViewModel>();


		public ICommand SettingsCommand => new Command(() => OnSettings?.Invoke());
		public ICommand OpenFolderBrowserDialogCommand => new Command(() => OpenFolderBrowserDialog());
		public ICommand SearchCommand => new Command(() => Search());
		public ICommand InstallCommand => new Command(() => InstallPackages());
		public ICommand UpdateCommand => new Command(() => UpdatePackages());
		public ICommand UninstallCommand => new Command(() => UninstallPackages());
		public ICommand ToggleProjectsSelectCommand => new Command<bool>((_) => ToggleProjectsSelect(_));
		public ICommand TogglePackagesSelectCommand => new Command<bool>((_) => TogglePackagesSelect(_));
		public ICommand OpenLogWindowsCommand => new Command(() => OpenLogWindows());
		public ICommand ShowUsedCommand => new Command<string>((packageId) => ShowUsedWindow(packageId));
		public ICommand OpenProjectLocationCommand => new Command<ProjectViewModel>((_) => OpenProjectLocation(_));
		public ICommand CheckAllPackagesCommand => new Command(() => CheckAllPackages());

		public IEnumerable<ProjectWithPackageModel> ProjectWithPackages { get; set; }

		public Action OnSettings { get; set; }
		public Action OnToggleProjectsSelect { get; set; }
		public Action OnTogglePackagesSelect { get; set; }

		public override Task InitializeAsync()
		{
			ProjectService.PackageUninstalling += ProjectService_PackageUninstalling;
			SearchPath = StorageHelper.SearchPath;
			SavedSearchPath.ReplaceRange(StorageHelper.GetSavedSearchPath());

			return base.InitializeAsync();
		}

		private void InitialFileWatcher()
		{
			// TODO
		}

		private void ProjectService_PackageUninstalling(string packageId)
		{
			LogMessage = $"Remove package {packageId}...";
		}

		public IEnumerable<string> GetSelectPackages()
		{
			return PackageList.Where(t => t.Selected).Select(t => t.Id).ToArray();
		}

		public IEnumerable<ProjectViewModel> GetSelectProjects()
		{
			return ProjectList.Where(t => t.Selected).ToArray();
		}

		private void ToggleProjectsSelect(bool value)
		{
			foreach (var item in ProjectList)
			{
				item.Selected = value;
			}
		}

		private void TogglePackagesSelect(bool value)
		{
			foreach (var item in PackageList)
			{
				item.Selected = value;
			}
		}

		private void OpenFolderBrowserDialog()
		{
			System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
			if (!string.IsNullOrEmpty(SearchPath))
				dialog.SelectedPath = SearchPath;

			dialog.Description = "Choose a solution folder";
			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				SearchPath = dialog.SelectedPath;
			}
		}

		protected void Search()
		{
			if (string.IsNullOrEmpty(SearchPath) || !Directory.Exists(SearchPath))
				return;

			StorageHelper.SearchPath = SearchPath;

			IsBusy = true;

			PackageList.Clear();
			ProjectList.Clear();

			// cancel
			_checkAllPackageCancellationTokenSource.Cancel();

			// 
			ProjectService.UpdateSolutionDirectory(SearchPath);

			Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
			{
				try
				{
					await LoadProjectsAndPackagesAsync();
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
				}

				IsBusy = false;
			}));
		}

		private async Task LoadProjectsAndPackagesAsync()
		{
			Logger.Log("Begin search projects...");

			LogMessage = "Search...";

			var projects = await ProjectService.SearchProjectsWithPackagesAsync(SearchPath, true, ProjectService.DefaultProjectTypes);

			Logger.Log("Find {0} projects.", projects.Count());

			ProjectService.UpdateSolutionProjects(projects);

			ProjectWithPackages = projects;

			var packages = projects.SelectMany(t => t.Packages).GroupBy(t => t.Id).Select(t => new PackageViewModel()
			{
				Id = t.Key,
				Versions = t.Select(p => p.Version).Distinct().ToArray(),
			}).OrderBy(t => t.Id).ToArray();

			PackageList.ReplaceRange(packages);

			ProjectList.ReplaceRange(projects.Select(t => new ProjectViewModel()
			{
				FolderPath = t.FolderPath,
				FrameworkName = t.FrameworkName,
				FullPath = t.FullPath,
				Name = t.Name,
				PackageCount = t.Packages?.Count() ?? 0,
			}).OrderBy(t => t.Name).ToArray());

			LogMessage = null;
		}

		private void InstallPackages()
		{
			var projects = GetSelectProjects();
			var packageIds = GetSelectPackages();

			if (projects.Any())
			{
				Application.Current.Dispatcher.BeginInvoke(new Action(() =>
				{
					_packagePreInstallWindowViewModel = new PackagePreInstallWindowViewModel(this)
					{
						ShowUsage = true
					};

					PackagePreInstallWindow preInstallWindow = new PackagePreInstallWindow(_packagePreInstallWindowViewModel);
					preInstallWindow.ShowDialog();
				}));
			}
		}

		private void UninstallPackages()
		{
			var projects = GetSelectProjects();
			var packageIds = GetSelectPackages();


			if (!projects.Any())
				return;

			string message = $"Are you sure you want to remove all packages from the {projects.Count()} selected projects?";
			if (packageIds.Any())
			{
				message = $"Are you sure you want to remove the selected {packageIds.Count()} packages from the selected {projects.Count()} projects?";
			}
			else
			{
				packageIds = projects.SelectMany(t => ProjectService.GetProjectPackages(t.FullPath, true)).Select(t => t.Id).ToArray();
			}

			if (!packageIds.Any())
			{
				MessageBox.Show("No packages can be removed.");
				return;
			}

			IsBusy = true;

			Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
			{
				var selectProjects = projects.Select(t => new ProjectModel()
				{
					Name = t.Name,
					FolderPath = t.FolderPath,
					FullPath = t.FullPath,
					FrameworkName = t.FrameworkName,
				}).ToArray();

				if (MessageBox.Show(message, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
				{
					if (_cancellationToken.IsCancellationRequested)
						_cancellationToken = new CancellationTokenSource();

					Stopwatch sw = Stopwatch.StartNew();

					bool success = false;
					try
					{
						await ProjectService.UninstallPackagesAsync(selectProjects, packageIds, new UninstallPackagesOptions(), _cancellationToken.Token);

						success = true;
					}
					catch (Exception ex)
					{
						if (!_cancellationToken.IsCancellationRequested)
							MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
						else
							Debug.WriteLine("Uninstall packages task is canceled.");
					}
					finally
					{
						sw.Stop();
						LogMessage = null;
					}

					if (success)
					{
						MessageBox.Show($"Done.({sw.Elapsed})");

						await LoadProjectsAndPackagesAsync();
					}
				}

				IsBusy = false;
			}));

		}

		private void UpdatePackages()
		{
			var projects = GetSelectProjects();
			var packageIds = GetSelectPackages();

			if (projects.Any() || packageIds.Any())
			{
				Application.Current.Dispatcher.BeginInvoke(new Action(() =>
				{
					_packagePreInstallWindowViewModel = new PackagePreInstallWindowViewModel(this)
					{
						ShowUsage = true,
						IsUpdateMode = true
					};

					PackagePreInstallWindow preInstallWindow = new PackagePreInstallWindow(_packagePreInstallWindowViewModel);
					preInstallWindow.ShowDialog();

				}));
			}
		}

		private LogsWindow _logsWindow = null;
		private int _percentageValue;

		private void OpenLogWindows()
		{
			if (_logsWindow == null)
				_logsWindow = new LogsWindow();

			if (_logsWindow.Visibility == Visibility.Visible && !_logsWindow.IsVisible)
			{
				_logsWindow = new LogsWindow();
			}

			_logsWindow.Show();
			_logsWindow.Activate();
		}

		private void ShowUsedWindow(string packageId)
		{
			var viewModel = new PackageUsedInfoWindowViewModel(this, packageId);
			var window = new PackageUsedInfoWindow(viewModel);
			window.Show();
		}

		private void OpenProjectLocation(ProjectViewModel item)
		{
			Process.Start(item.FolderPath);
		}

		private void CheckAllPackages()
		{
			if (!PackageList.Any())
				return;

			IsBusy = true;

			Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
			{
				await CheckAllPackagesCoreAsync();

				IsBusy = false;
			}));
		}

		private async Task CheckAllPackagesCoreAsync()
		{
			var tasks = new List<Task>();

			var sourceCacheContext = ProjectService.NuGetPackageService.CreateSourceCacheContext();

			foreach (var item in PackageList)
			{
				tasks.Add(CheckPackagesCoreAsync(item, sourceCacheContext));
			}

			await Task.WhenAll(tasks);
		}

		private async Task CheckPackagesCoreAsync(PackageViewModel item, SourceCacheContext sourceCacheContext)
		{
			try
			{
				var latestVersion = await ProjectService.NuGetPackageService.GetNuGetLatestVersionAsync(item.Id, sourceCacheContext, true, false, default);

				if (latestVersion != null)
				{
					item.LatestVersion = latestVersion;

					if (item.Versions.Any())
					{
						var versions = item.Versions.Select(v => NuGetVersion.Parse(v));
						if (versions.OrderByDescending(t => t).First() < latestVersion)
						{
							item.CanUpdate = true;
						}

						if (item.Versions.Distinct().Count() != 1)
						{
							item.CanConsolidate = true;
						}
					}
				}
			}
			catch (Exception)
			{

			}
		}

	}

	public class ProjectViewModel : BaseViewModel
	{
		private string _name;
		private string _folderPath;
		private string _fullPath;
		private string _frameworkName;
		private bool _selected;
		private int _packageCount;

		public string Name { get => _name; set => Set(ref _name, value); }
		public string FolderPath { get => _folderPath; set => Set(ref _folderPath, value); }
		public string FullPath { get => _fullPath; set => Set(ref _fullPath, value); }
		public string FrameworkName { get => _frameworkName; set => Set(ref _frameworkName, value); }
		public bool Selected { get => _selected; set => Set(ref _selected, value); }
		public int PackageCount { get => _packageCount; set => Set(ref _packageCount, value); }
	}

	public class PackageViewModel : BaseViewModel
	{
		private string _id;
		private bool _selected;
		private bool _canUpdate;
		private bool _canConsolidate;

		public string Id { get => _id; set => Set(ref _id, value); }
		public bool Selected { get => _selected; set => Set(ref _selected, value); }

		public bool CanUpdate { get => _canUpdate; set => Set(ref _canUpdate, value); }
		public bool CanConsolidate { get => _canConsolidate; set => Set(ref _canConsolidate, value); }

		public IEnumerable<string> Versions { get; set; }

		public NuGetVersion LatestVersion { get; set; }

	}
}
