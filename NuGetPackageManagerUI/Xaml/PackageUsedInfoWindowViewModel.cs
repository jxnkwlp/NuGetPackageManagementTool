using NuGetPackageManagerUI.Models;
using NuGetPackageManagerUI.Services;
using NuGetPackageManagerUI.Utils;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NuGetPackageManagerUI.Xaml
{
	public class PackageUsedInfoWindowViewModel : BaseViewModel
	{
		private readonly MainWindowViewModel _parentViewModel;
		private readonly string _packageId;
		private bool _showProjectPath = false;

		public bool ShowProjectPath { get => _showProjectPath; set => Set(ref _showProjectPath, value); }


		//public ICommand ToggleShowProjectPath => new Command<bool>((value) => ShowProjectPath = value);
		public ICommand OpenProjectLocationCommand => new Command<ProjectPackageUsedItemViewModel>((_) => OpenProjectLocation(_));

		public ObservableRangeCollection<ProjectPackageUsedItemViewModel> Items { get; set; } = new ObservableRangeCollection<ProjectPackageUsedItemViewModel>();

		public PackageUsedInfoWindowViewModel()
		{
		}

		public PackageUsedInfoWindowViewModel(MainWindowViewModel parentViewModel, string packageId)
		{
			_parentViewModel = parentViewModel;
			_packageId = packageId;
			this.Title = $"Package used - {packageId}";
		}

		public override async Task InitializeAsync()
		{
			var projectFiles = await SolutionDiretoryManager.GetProjectFilesAsync();
			var installedPackages = await GetInstalledPackagesFromProjectFilesAsync2(projectFiles);

			var installedPackages2 = installedPackages.Where(t => t.Value.Any(p => string.Equals(p.Id, _packageId, StringComparison.InvariantCultureIgnoreCase)));

			var list = installedPackages2.Select(t => new ProjectPackageUsedItemViewModel()
			{
				Project = t.Key.ToProjectModel(),

				PackageVersion = t.Value.FirstOrDefault(p => p.Id.Equals(_packageId, StringComparison.InvariantCultureIgnoreCase))?.Version.ToNormalizedString(),
			});

			var orderList = list.OrderByDescending(t => t.PackageVersion, new PackageVesionStringComparer()).ThenBy(t => t.Project.Name).ToArray();

			Items.ReplaceRange(orderList);

		}

		private void OpenProjectLocation(ProjectPackageUsedItemViewModel item)
		{
			Process.Start(item.Project.FolderPath);
		}
	}

	public class ProjectPackageUsedItemViewModel : BaseViewModel
	{
		public ProjectModel Project { get; set; }
		public string PackageVersion { get; set; }
	}
}
