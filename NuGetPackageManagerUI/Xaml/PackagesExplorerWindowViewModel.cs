using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NuGetPackageManagerUI.Xaml
{
	public class PackagesExplorerWindowViewModel : BaseViewModel
	{
		private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
		private readonly PackagePreInstallWindowViewModel _parentViewModel;
		private string _searchTerm;
		private string _sourceRepositoryName;
		private bool _includePrerelease;
		private bool _showLoading;
		private int _skip = 0;
		private IPackageSearchMetadata _selectItem;
		private readonly int _take = 20;

		public string SourceRepositoryName { get => _sourceRepositoryName; set => Set(ref _sourceRepositoryName, value); }
		public string SearchTerm { get => _searchTerm; set => Set(ref _searchTerm, value); }
		public bool IncludePrerelease { get => _includePrerelease; set => Set(ref _includePrerelease, value); }
		public bool ShowLoading { get => _showLoading; set => Set(ref _showLoading, value); }

		public IPackageSearchMetadata SelectItem { get => _selectItem; set => Set(ref _selectItem, value); }

		public ObservableRangeCollection<string> SourceRepositories { get; set; } = new ObservableRangeCollection<string>();
		public ObservableRangeCollection<IPackageSearchMetadata> Packages { get; set; } = new ObservableRangeCollection<IPackageSearchMetadata>();

		public ICommand SearchCommand => new Command(() => Search());
		public ICommand PackageItemSelectedCommand => new Command(() => PackageItemSelected());
		public ICommand IncludePrereleaseToggleCommand => new Command(() => IncludePrereleaseToggle());
		public ICommand SourceRepositoryChangeCommand => new Command(() => SourceRepositoryChange());
		public ICommand CancelLoadCommand => new Command(() => CancelLoad());

		public PackagesExplorerWindowViewModel(PackagePreInstallWindowViewModel parentViewModel)
		{
			_parentViewModel = parentViewModel;
		}

		// for view
		public PackagesExplorerWindowViewModel()
		{
		}

		public override Task InitializeAsync()
		{
			SourceRepositories.ReplaceRange(NuGetPackageService.PrimarySourcesRepository.Select(t => t.PackageSource.Name).ToArray());

			return base.InitializeAsync();
		}

		private void Search()
		{
			LoadPackages();
		}

		private void IncludePrereleaseToggle()
		{
			LoadPackages();
		}

		private void SourceRepositoryChange()
		{
			LoadPackages();
		}

		private void CancelLoad()
		{
			_cancellationToken.Cancel();
		}

		public void LoadPackages(bool isNext = false)
		{
			if (IsBusy)
				return;
			IsBusy = true;

			if (isNext)
			{
				_skip += _take;
				// 
				if (_cancellationToken.IsCancellationRequested)
					return;
			}
			else
			{
				_skip = 0;
				Packages.Clear();
			}

			if (_cancellationToken.IsCancellationRequested)
				_cancellationToken = new CancellationTokenSource();

			Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
			{
				if (Packages.Count == 0)
					ShowLoading = true;

				try
				{
					var list = await NuGetPackageService.SearchPackagesAsync(SourceRepositoryName, SearchTerm, _skip, _take, IncludePrerelease, null, _cancellationToken.Token);

					ShowLoading = false;

					Packages.AddRange(list);
				}
				catch (Exception ex)
				{
					ShowLoading = false;

					if (_cancellationToken.IsCancellationRequested)
						Debug.WriteLine("Task is canceled.");
					else
						MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}

				IsBusy = false;
			}));
		}

		private void PackageItemSelected()
		{
			var selectItem = SelectItem;
			if (selectItem != null)
			{
				CloseWindow();
			}
		}

		public class PackageSearchMetadataModel : PackageSearchMetadata
		{
		}
	}
}
