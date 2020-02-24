using NuGetPackageManagerUI.NuGet;
using NuGetPackageManagerUI.Services;
using NuGetPackageManagerUI.Utils;
using System.Windows;
using System;

namespace NuGetPackageManagerUI
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			var nuGetLogger = new DefaultNuGetLogger();
			ServiceLocator.RegisterService<global::NuGet.Common.ILogger>(nuGetLogger);

			ServiceLocator.RegisterService(new ProjectService());

			ILogger appLogger = new SimpleLogger();
			ServiceLocator.RegisterService<ILogger>(appLogger);


			nuGetLogger.OnLog += (level, message) =>
			{
				appLogger.Log(message);
			};
		}
	}
}
