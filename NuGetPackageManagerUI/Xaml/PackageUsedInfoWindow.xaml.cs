using System;
using System.Windows;

namespace NuGetPackageManagerUI.Xaml
{
	/// <summary>
	/// Interaction logic for PackageUsedInfoWindow.xaml
	/// </summary>
	public partial class PackageUsedInfoWindow : Window
	{
		private readonly PackageUsedInfoWindowViewModel _vm;
		private bool _init = false;

		public PackageUsedInfoWindow()
		{
			InitializeComponent();

			_vm = DataContext as PackageUsedInfoWindowViewModel;
		}

		public PackageUsedInfoWindow(PackageUsedInfoWindowViewModel viewModel)
		{
			InitializeComponent();

			DataContext = _vm = viewModel;
		}

		protected override void OnContentRendered(EventArgs e)
		{
			base.OnContentRendered(e);
			if (_init) return;
			_init = true;

			KeyUp += PackageUsedInfoWindow_KeyUp;

			Init();
		}

		private void PackageUsedInfoWindow_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Escape)
			{
				e.Handled = true;
				Close();
			}
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
	}
}
