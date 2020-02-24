using System;
using System.ComponentModel;
using System.Windows;

namespace NuGetPackageManagerUI.Xaml
{
	/// <summary>
	/// Interaction logic for PackagePreInstallWindow.xaml
	/// </summary>
	public partial class PackagePreInstallWindow
	{
		private bool _init = false;
		private readonly PackagePreInstallWindowViewModel _vm = null;

		public PackagePreInstallWindow()
		{
			InitializeComponent();
			_vm = DataContext as PackagePreInstallWindowViewModel;
		}

		public PackagePreInstallWindow(PackagePreInstallWindowViewModel vm)
		{
			InitializeComponent();
			DataContext = _vm = vm;
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

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			if (_vm.IsBusy && MessageBox.Show("There are currently operations in progress. Are you sure you want to cancel and exit?", "Confirm", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) != MessageBoxResult.Yes)
			{
				e.Cancel = true;
			}
			else
			{
				_vm.OnWindowsClosing();
			}
		}

	}
}
