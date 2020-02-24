using System;
using System.Diagnostics;
using System.Windows;

namespace NuGetPackageManagerUI.Xaml
{
	/// <summary>
	/// Interaction logic for PackagesExplorerWindow.xaml
	/// </summary>
	public partial class PackagesExplorerWindow : Window
	{
		private bool _init;
		private readonly PackagesExplorerWindowViewModel _vm = null;

		public PackagesExplorerWindow()
		{
			InitializeComponent();
			_vm = this.DataContext as PackagesExplorerWindowViewModel;
		}

		public PackagesExplorerWindow(PackagesExplorerWindowViewModel vm)
		{
			InitializeComponent();
			this.DataContext = _vm = vm;
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

		private void TxtSearchTerm_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			e.Handled = false;
			if (e.Key == System.Windows.Input.Key.Enter)
			{
				_vm.SearchCommand?.Execute(null);
			}
		}

		private void ListView1_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
		{
			//Debug.WriteLine($"ExtentHeight{e.ExtentHeight}  ViewportHeight:{e.ViewportHeight}  VerticalOffset：{e.VerticalOffset}");

			if (e.VerticalOffset < e.ViewportHeight - 10)
				return;

			if (e.ViewportHeight + e.VerticalOffset > e.ExtentHeight - 10)
			{
				_vm.LoadPackages(true);
			}
		}

		private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			_vm.LoadPackages(false);
		}
	}
}
