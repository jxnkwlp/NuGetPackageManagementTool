using System.Windows;

namespace NuGetPackageManagerUI
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			DispatcherHelper.Initialize();
			ServiceLocator.Initial();
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
		}


	}
}
