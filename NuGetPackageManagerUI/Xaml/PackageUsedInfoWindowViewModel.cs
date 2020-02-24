using NuGetPackageManagerUI.Models;
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

		public override Task InitializeAsync()
		{
			var projects = _parentViewModel.ProjectWithPackages.Where(t => t.Packages.Any(p => p.Id.Equals(_packageId, StringComparison.InvariantCultureIgnoreCase)));

			var list = projects.Select(t => new ProjectPackageUsedItemViewModel()
			{
				Project = new ProjectModel()
				{
					Name = t.Name,
					FullPath = t.FullPath,
					FolderPath = t.FolderPath,
					FrameworkName = t.FrameworkName,
				},

				Package = t.Packages.FirstOrDefault(p => p.Id.Equals(_packageId, StringComparison.InvariantCultureIgnoreCase)),
			});

			var orderList = list.OrderByDescending(t => t.Package.Version, new PackageVesionStringComparer()).ThenBy(t => t.Project.Name).ToArray();

			Items.ReplaceRange(orderList);

			return Task.CompletedTask;
		}

		private void OpenProjectLocation(ProjectPackageUsedItemViewModel item)
		{
			Process.Start(item.Project.FolderPath);
		}
	}

	public class ProjectPackageUsedItemViewModel : BaseViewModel
	{
		public ProjectModel Project { get; set; }
		public PackageModel Package { get; set; }
	}
}
