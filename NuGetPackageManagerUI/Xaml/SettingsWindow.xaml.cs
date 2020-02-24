using System;
using System.Windows;

namespace NuGetPackageManagerUI.Xaml
{
	/// <summary>
	/// Interaction logic for SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window
	{
		private readonly SettingsWindowViewModel _vm = new SettingsWindowViewModel();
		private bool _init;

		public SettingsWindow()
		{
			InitializeComponent();
			_vm = DataContext as SettingsWindowViewModel;

			_vm.OnCloseWindow = () =>
			{
				Close();
			};
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
	}
}
