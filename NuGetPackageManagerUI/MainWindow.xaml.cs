using NuGetPackageManagerUI.Models;
using NuGetPackageManagerUI.Services;
using NuGetPackageManagerUI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace NuGetPackageManagerUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly MainWindowViewModel _vm;
		private readonly PackagePreInstallWindowViewModel _packagePreInstallWindowViewModel;
		private bool _init;
		 
		public MainWindow()
		{
			InitializeComponent();
			_vm = DataContext as MainWindowViewModel;

			InitViewModel();
		}

		protected override void OnContentRendered(EventArgs e)
		{
			base.OnContentRendered(e);
			if (_init) return;
			_init = true;

			Init();
		}

		private void Init()
		{
			_vm.CloseWindowAction = () =>
			{
				_vm.OnWindowsClosing();
				Close();
			};

			_vm.IsBusy = true;

			Dispatcher.BeginInvoke(new Action(async () =>
			{
				await _vm.InitializeAsync();

				_vm.IsBusy = false;

			}), System.Windows.Threading.DispatcherPriority.Background);
		}

		private void InitViewModel()
		{
			_vm.OnSettings = OnSettings;
		}

		//private void OnProjectsSearch(string path)
		//{
		//	if (string.IsNullOrEmpty(_vm.SearchPath) || !Directory.Exists(_vm.SearchPath))
		//		return;

		//	//ProjectService.MsbuildDirectory = StorageHelper.MsBuildDirectory;

		//	//if (string.IsNullOrEmpty(ProjectService.MsbuildDirectory))
		//	//{
		//	//	MessageBox.Show("Please open the settings first and fill the Ms Build Directory field.");
		//	//	return;
		//	//}

		//	StorageHelper.SearchPath = (_vm.SearchPath);

		//	_vm.IsBusy = true;

		//	_vm.PackageList.Clear();
		//	_vm.ProjectList.Clear();

		//	// 
		//	ProjectService.UpdateSolutionDirectory(_vm.SearchPath);

		//	Dispatcher.BeginInvoke(new Action(async () =>
		//	{
		//		try
		//		{
		//			var projects = await ProjectService.SearchProjectsWithPackagesAsync(_vm.SearchPath, true, ProjectService.DefaultProjectTypes);

		//			ProjectService.UpdateSolutionProjects(projects.Select(t => t.Project));

		//			_vm.ProjectWithPackages = projects;

		//			_vm.PackageList.ReplaceRange(projects.SelectMany(t => t.Packages.Select(p => p.Id)).Distinct().OrderBy(t => t).Select(t => new PackageViewModel() { Id = t }));

		//			_vm.ProjectList.ReplaceRange(projects.Select(t => new ProjectViewModel()
		//			{
		//				FolderPath = t.Project.FolderPath,
		//				FrameworkName = t.Project.FrameworkName,
		//				FullPath = t.Project.FullPath,
		//				Name = t.Project.Name,
		//			}).OrderBy(t => t.Name));

		//			//var projects = await ProjectService.SearchProjectsAsync(_vm.SearchPath, true, ProjectService.DefaultProjectTypes);

		//			//var packages = await GetPackagesAsync(projects);

		//			//_vm.PackageList.ReplaceRange(packages.Select(t => t.Id).Distinct().OrderBy(t => t).Select(t => new PackageViewModel() { Id = t, }));

		//			//_vm.ProjectList.ReplaceRange(projects.Select(t => new ProjectViewModel()
		//			//{
		//			//	FolderPath = t.FolderPath,
		//			//	FrameworkName = t.FrameworkName,
		//			//	FullPath = t.FullPath,
		//			//	Name = t.Name,
		//			//}).OrderBy(t => t.Name));

		//		}
		//		catch (Exception ex)
		//		{
		//			MessageBox.Show(ex.Message);
		//		}

		//		_vm.IsBusy = false;
		//	}));
		//}

		private static Task<IEnumerable<PackageModel>> GetPackagesAsync(IEnumerable<ProjectModel> projects)
		{
			return Task.Run(() => projects.SelectMany(t => ProjectService.GetProjectPackages(t.FullPath, true)));
		}

		//private void OnInstall()
		//{
		//	var projects = _vm.GetSelectProjects();
		//	var packageIds = _vm.GetSelectPackages();

		//	if (projects.Any())
		//	{
		//		this.Dispatcher.BeginInvoke(new Action(() =>
		//		{
		//			_packagePreInstallWindowViewModel = new PackagePreInstallWindowViewModel(this._vm);
		//			_packagePreInstallWindowViewModel.ShowUsage = true;

		//			PackagePreInstallWindow preInstallWindow = new PackagePreInstallWindow(_packagePreInstallWindowViewModel);
		//			preInstallWindow.ShowDialog();

		//		}));
		//	}

		//}
		 
		private void OnSettings()
		{
			new SettingsWindow().ShowDialog();
		}
	}

}
