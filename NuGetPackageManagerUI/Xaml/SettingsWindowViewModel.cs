using NuGetPackageManagerUI.Utils;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NuGetPackageManagerUI.Xaml
{
	public class SettingsWindowViewModel : BaseViewModel
	{
		private string _msBuildDirectory;

		public string MsBuildDirectory { get => _msBuildDirectory; set => Set(ref _msBuildDirectory, value); }

		public ICommand SubmitCommand => new Command(() => Submit());

		public ICommand DiscoverMsBuildDirectoryCommand => new Command(() => DiscoverMsBuildDirectory());

		public Action OnCloseWindow { get; set; }

		public override Task InitializeAsync()
		{
			this.MsBuildDirectory = StorageHelper.MsBuildDirectory;
			return base.InitializeAsync();
		}

		private void Submit()
		{
			StorageHelper.MsBuildDirectory = MsBuildDirectory;

			OnCloseWindow?.Invoke();
		}

		private void DiscoverMsBuildDirectory()
		{
			Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
			{
				try
				{
					MsBuildDirectory = await VsMsBuildHelper.FindVsMsBulidLocationAsync();
				}
				catch (Exception)
				{
					MessageBox.Show("Ms build directory not found, you installed visualstudio?");
				}

				if (string.IsNullOrEmpty(MsBuildDirectory))
				{
					MessageBox.Show("Ms build directory not found, you installed visualstudio?");
				}
			}));
		}
	}
}
