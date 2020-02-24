using NuGet.Versioning;
using NuGetPackageManagerUI.Models;
using NuGetPackageManagerUI.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NuGetPackageManagerUI.Xaml
{
	public class PackagePreInstallWindowViewModel : BaseViewModel
	{
		//private readonly IEnumerable<string> _initPackageIds;
		private readonly MainWindowViewModel _parentViewModel;
		private bool _includePrerelease;
		private string _logMessage;
		private int _percentageValue;

		private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
		private bool _canAddPackage;

		public bool IncludePrerelease { get => _includePrerelease; set => Set(ref _includePrerelease, value); }
		public string LogMessage { get => _logMessage; set => Set(ref _logMessage, value); }
		public int PercentageValue { get => _percentageValue; set => Set(ref _percentageValue, value); }
		public bool CanAddPackage { get => _canAddPackage; set => Set(ref _canAddPackage, value); }

		public ObservableRangeCollection<PackageInfoViewModel> Packages { get; set; } = new ObservableRangeCollection<PackageInfoViewModel>();


		public bool ShowUsage { get; set; }

		public bool IsUpdateMode { get; set; }

		public IEnumerable<ProjectWithPackageModel> ProjectWithPackages => _parentViewModel.ProjectWithPackages;

		public ICommand ToggleIncludePrereleaseCommand => new Command(() => { });
		public ICommand ToggleIncludePrereleaseChangeCommand => new Command<bool>((value) => ToggleIncludePrereleaseChange(value));

		public ICommand AddNewPackageCommand => new Command(() => AddNewPackages());
		public ICommand ToggleUsedExpandedCommand => new Command(() =>
		{
			var flag = Packages.Any(t => t.ShowUsage);

			foreach (var item in Packages)
			{
				item.ShowUsage = !flag;
			}
		});
		public ICommand ChooseLatestStableVersionCommand => new Command(() =>
		{
			foreach (var item in Packages)
			{
				if (!string.IsNullOrEmpty(item.LatestStableVersion))
					item.Version = item.LatestStableVersion;
			}
		});
		public ICommand ChooseLatestVersionCommand => new Command(() =>
		{
			foreach (var item in Packages)
			{
				if (!string.IsNullOrEmpty(item.LatestVersion))
					item.Version = item.LatestVersion;
			}
		});
		public ICommand ChooseLatestProjectVersionCommand => new Command(() =>
		{
			foreach (var item in Packages)
			{
				if (!string.IsNullOrEmpty(item.LatestInstalledVersion))
					item.Version = item.LatestInstalledVersion;
			}
		});

		public ICommand ReloadPackageCommand => new Command<string>((id) => ReloadPackageData(id));
		public ICommand RemovePackageCommand => new Command<string>((id) => RemovePackage(id));

		public ICommand SubmitCommand => new Command(() => Submit());




		public PackagePreInstallWindowViewModel(MainWindowViewModel parentViewModel)
		{
			_parentViewModel = parentViewModel;

			ProjectService.PackageInstalling += (packageId, version) =>
			{
				LogMessage = $"Installing package '{packageId}.{version}'";
			};
		}

		// for view
		public PackagePreInstallWindowViewModel()
		{
		}

		public override async Task InitializeAsync()
		{
			CanAddPackage = !IsUpdateMode;

			InitPackages();

			await Task.Delay(1000);

			await ReloadPackagesUsageAndAllVersionAsync();
		}

		public void Submit()
		{
			Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
			{
				await SubmitCoreAsync();
			}), System.Windows.Threading.DispatcherPriority.Background);
		}

		private async Task SubmitCoreAsync()
		{
			if (Packages.Any(t => string.IsNullOrEmpty(t.Version)))
			{
				var package = Packages.First(t => string.IsNullOrEmpty(t.Version));
				MessageBox.Show($"The package '{package.Id}' need choose version.", "Notice", MessageBoxButton.OK);
				return;
			}

			if (_cancellationToken.IsCancellationRequested)
				_cancellationToken = new CancellationTokenSource();

			IsBusy = true;

			Stopwatch sw = Stopwatch.StartNew();

			(bool, Exception) result;

			// is update mode , mean just change package version.
			// otherwise , install package to project if not exist.
			if (IsUpdateMode)
			{
				result = await UpdatePackagesAsync();
			}
			else
			{
				result = await InstallPackagesAsync();
			}

			sw.Stop();

			IsBusy = false;
			LogMessage = "";

			if (result.Item1)
			{
				MessageBox.Show($"Done.({sw.Elapsed})");

				CloseWindow();
			}
			else if (result.Item2 != null)
			{
				if (!_cancellationToken.IsCancellationRequested)
					MessageBox.Show(result.Item2.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				else
					Debug.WriteLine("Install packages task is canceled.");
			}
		}

		private async Task<(bool, Exception)> InstallPackagesAsync()
		{
			var projects = _parentViewModel.GetSelectProjects().Select(t => new ProjectModel()
			{
				Name = t.Name,
				FolderPath = t.FolderPath,
				FullPath = t.FullPath,
				FrameworkName = t.FrameworkName,
			}).ToArray();

			var packages = Packages.Select(t => new PackageModel()
			{
				Id = t.Id,
				Version = t.Version,
			}).ToArray();

			bool success = false;
			Exception exception = null;

			try
			{
				await ProjectService.InstallPackagesAsync(projects, packages, new InstallPackagesOptions() { IgnoreDependencies = false, IncludePrerelease = true, }, _cancellationToken.Token);

				success = true;
			}
			catch (Exception ex)
			{
				exception = ex;
			}

			return (success, exception);
		}

		private async Task<(bool, Exception)> UpdatePackagesAsync()
		{
			IEnumerable<ProjectModel> projects = null;

			if (_parentViewModel.GetSelectProjects().Any())
			{
				projects = _parentViewModel.GetSelectProjects().Select(t => new ProjectModel()
				{
					Name = t.Name,
					FolderPath = t.FolderPath,
					FullPath = t.FullPath,
					FrameworkName = t.FrameworkName,
				}).ToArray();
			}
			else
			{
				projects = _parentViewModel.ProjectList.Select(t => new ProjectModel()
				{
					Name = t.Name,
					FolderPath = t.FolderPath,
					FullPath = t.FullPath,
					FrameworkName = t.FrameworkName,
				}).ToArray();
			}

			var packages = Packages.Select(t => new PackageModel()
			{
				Id = t.Id,
				Version = t.Version,
			}).ToArray();

			bool success = false;
			Exception exception = null;

			try
			{
				await ProjectService.UpdatePackagesAsync(projects, packages, new UpdatePackagesOptions(), _cancellationToken.Token);

				success = true;
			}
			catch (Exception ex)
			{
				exception = ex;
			}

			return (success, exception);
		}

		public async Task ReloadPackagesUsageAndAllVersionAsync()
		{
			IsBusy = true;

			var totalCount = Packages.Count;

			for (int i = 0; i < totalCount; i++)
			{
				PercentageValue = (i * 100) / totalCount;

				await LoadPackageDataAsync(Packages[i]);
				//#if DEBUG
				//		await Task.Delay(500);
				//#endif
			}

			LogMessage = "";
			PercentageValue = 0;

			IsBusy = false;
		}

		private void InitPackages()
		{
			if (IsUpdateMode)
			{
				var projects = _parentViewModel.GetSelectProjects();
				IEnumerable<string> packageIds;

				if (_parentViewModel.GetSelectPackages().Any())
				{
					packageIds = _parentViewModel.GetSelectPackages();
				}
				else
				{
					packageIds = _parentViewModel.ProjectWithPackages.Where(t => projects.Any(p => p.FullPath == t.FullPath)).SelectMany(t => t.Packages.Select(c => c.Id));
				}

				if (!packageIds.Any())
				{
					if (MessageBox.Show("No packages!", "Notice", MessageBoxButton.OK, MessageBoxImage.Information) == MessageBoxResult.OK)
					{
						CloseWindow();
					}
					return;
				}
				InitPackagesCore(packageIds);
			}
			else
			{
				var packageIds = _parentViewModel.GetSelectPackages();
				InitPackagesCore(packageIds);
			}
		}

		private void InitPackagesCore(IEnumerable<string> packageIds)
		{
			var packages = packageIds.Select(t => new PackageInfoViewModel()
			{
				Id = t,
				EnableUsage = ShowUsage,
				IsBusy = true,
			});

			// update list 
			Packages.ReplaceRange(packages);
		}

		private async Task LoadPackageDataAsync(PackageInfoViewModel package)
		{
			package.IsBusy = true;
			package.HasError = false;

			LogMessage = $"Loading package '{package.Id}' ";

			var used = GetPackageUsage(ProjectWithPackages, package.Id);

			if (_cancellationToken.IsCancellationRequested)
				_cancellationToken = new CancellationTokenSource();

			try
			{
				var allVerions = await ProjectService.NuGetPackageService.GetNuGetVersionsAsync(package.Id, _cancellationToken.Token);
				if (!IncludePrerelease)
					allVerions = allVerions.Where(t => !t.IsPrerelease);

				allVerions = allVerions.OrderByDescending(t => t);
				if (allVerions.Any())
				{
					package.Versions.ReplaceRange(allVerions.Select(t => t.ToNormalizedString()).ToArray());

					package.LatestStableVersion = allVerions.FirstOrDefault(t => !t.IsPrerelease)?.ToNormalizedString();
					package.LatestVersion = allVerions.First().ToNormalizedString();

					package.Version = allVerions.First().ToNormalizedString();
				}

				if (used.Any())
					package.LatestInstalledVersion = used.Values.Select(t => NuGetVersion.Parse(t.Version)).OrderByDescending(t => t).First().ToNormalizedString();
			}
			catch (Exception ex)
			{
				package.HasError = true;
				package.ErrorMessage = ex.Message;
			}

			//   
			if (package.EnableUsage)
			{
				package.UsageText = FormatUsage(used);
			}

			package.IsBusy = false;
		}

		private void ToggleIncludePrereleaseChange(bool value)
		{
			System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
			{
				await ReloadPackagesUsageAndAllVersionAsync();
			}));
		}

		private void RemovePackage(string packageId)
		{
			var package = Packages.FirstOrDefault(t => t.Id == packageId);

			if (package == null)
				return;

			System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
			{
				if (MessageBox.Show($"Are you sure remove '{packageId}' ?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
				{
					Packages.Remove(package);
				}
			}));

		}

		private void ReloadPackageData(string packageId)
		{
			var package = Packages.FirstOrDefault(t => t.Id == packageId);

			if (package == null)
				return;

			IsBusy = true;
			package.IsBusy = true;

			System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
			{
				await LoadPackageDataAsync(package);

				package.IsBusy = false;
				IsBusy = false;
			}));
		}

		public static string FormatUsage(Dictionary<ProjectModel, PackageModel> used)
		{
			if (used.Any())
				return string.Join(Environment.NewLine, used.Select(t => $"{t.Key.Name}: {t.Value.Version}"));
			return null;
		}

		public static Dictionary<ProjectModel, PackageModel> GetPackageUsage(IEnumerable<ProjectWithPackageModel> source, string packageId)
		{
			var find = source.Where(t => t.Packages.Any(p => p.Id.Equals(packageId, StringComparison.InvariantCultureIgnoreCase))).Select(t => new { Project = t, Package = t.Packages.FirstOrDefault(p => p.Id.Equals(packageId, StringComparison.InvariantCultureIgnoreCase)) });

			return find.ToDictionary(t => (ProjectModel)t.Project, t => t.Package);
		}

		public override void OnWindowsClosing()
		{
			_cancellationToken.Cancel();
		}

		private void AddNewPackages()
		{
			Application.Current.Dispatcher.BeginInvoke(new Action(() =>
			{
				PackagesExplorerWindowViewModel viewModel = new PackagesExplorerWindowViewModel(this);

				PackagesExplorerWindow window = new PackagesExplorerWindow(viewModel);

				window.Closed += async (sender, e) =>
				{
					if (viewModel.SelectItem != null)
					{
						IsBusy = true;

						var id = viewModel.SelectItem.Identity.Id;
						var version = viewModel.SelectItem.Identity.Version.ToNormalizedString();

						if (!Packages.Any(t => t.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase)))
						{
							var packageItem = new PackageInfoViewModel()
							{
								IsBusy = true,
								EnableUsage = false,
								Id = viewModel.SelectItem.Identity.Id,
								Version = viewModel.SelectItem.Identity.Version.ToNormalizedString()
							};

							Packages.Add(packageItem);

							await LoadPackageDataAsync(packageItem);
						}

						LogMessage = "";
						IsBusy = false;
					}
				};

				window.ShowDialog();
			}));
		}
	}

	public class PackageInfoViewModel : BaseViewModel
	{
		private string _id;
		private string _version;
		private bool _showUsage;
		private bool _enableUsage;
		private string _usageText;
		private bool _hasError;
		private string _errorMessage;

		public string Id { get => _id; set => Set(ref _id, value); }
		public string Version { get => _version; set => Set(ref _version, value); }
		public bool ShowUsage { get => _showUsage; set => Set(ref _showUsage, value); }
		public bool EnableUsage { get => _enableUsage; set => Set(ref _enableUsage, value); }
		public string UsageText { get => _usageText; set => Set(ref _usageText, value); }
		public bool HasError { get => _hasError; set => Set(ref _hasError, value); }
		public string ErrorMessage { get => _errorMessage; set => Set(ref _errorMessage, value); }

		public ObservableRangeCollection<string> Versions { get; set; } = new ObservableRangeCollection<string>();

		public string LatestVersion { get; set; }
		public string LatestStableVersion { get; set; }
		public string LatestInstalledVersion { get; set; }
	}
}
