using Microsoft.Extensions.DependencyInjection;
using NuGet.PackageManagement;
using NuGetPackageManagerUI.Services;
using NuGetPackageManagerUI.Services.NuGets;
using NuGetPackageManagerUI.Services.NuGets.Projects;
using NuGetPackageManagerUI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuGetPackageManagerUI
{
	public static class ServiceLocator
	{
		public static IServiceProvider ServiceProvider { get; private set; }

		static ServiceLocator()
		{
			var serviceCollection = new ServiceCollection();
			ConfigureServices(serviceCollection);

			ServiceProvider = serviceCollection.BuildServiceProvider();
		}

		public static void Initial() { }

		private static void ConfigureServices(IServiceCollection services)
		{
			var nuGetLogger = new DefaultNuGetLogger();
			services.AddSingleton<global::NuGet.Common.ILogger>(nuGetLogger);

			ILogger appLogger = new SimpleLogger();
			services.AddSingleton<ILogger>(appLogger);

			nuGetLogger.OnLog += (level, message) =>
			{
				appLogger.Log(message);
			};

			services.AddTransient(typeof(Lazy<>), typeof(Lazier<>));

			ScanAndRegisterServies(services);

			// services 
			//services.AddSingleton<ISolutionDiretoryManager, SolutionDiretoryManager>();
			//services.AddSingleton<IProjectService, ProjectService>();
			//services.AddSingleton<ISolutionManager, MySolutionManager>();
			//services.AddTransient<NuGetProjectFactory>();
			//services.AddTransient<INuGetProjectProvider,>();
			//services.AddSingleton<ISolutionDiretoryManager, SolutionDiretoryManager>();
			//services.AddSingleton<ISolutionDiretoryManager, SolutionDiretoryManager>();

			// viewmodel
			services.AddScoped<MainWindowViewModel>();
		}

		private static void ScanAndRegisterServies(IServiceCollection services)
		{
			var allTypes = typeof(ServiceLocator).Assembly.GetTypes().Where(t => t.IsClass && !t.IsInterface && !t.IsAbstract);
			foreach (var type in allTypes)
			{
				var interfaces = type.GetInterfaces();
				if (interfaces.Contains(typeof(ISingletonService)))
				{
					if (type.GetInterfaces().Length == 1)
					{
						services.AddSingleton(type);
					}
					else
						foreach (var @interfaceType in type.GetInterfaces().Where(t => t != typeof(ISingletonService)))
						{
							services.AddSingleton(@interfaceType, type);
						}
				}
				else if (interfaces.Contains(typeof(IScopedService)))
				{
					if (type.GetInterfaces().Length == 1)
					{
						services.AddScoped(type);
					}
					else
						foreach (var @interfaceType in type.GetInterfaces().Where(t => t != typeof(IScopedService)))
						{
							services.AddScoped(@interfaceType, type);
						}
				}
				else if (interfaces.Contains(typeof(ITransientService)))
				{
					if (type.GetInterfaces().Length == 1)
					{
						services.AddTransient(type);
					}
					else
						foreach (var @interfaceType in type.GetInterfaces().Where(t => t != typeof(ITransientService)))
						{
							services.AddTransient(@interfaceType, type);
						}
				}
			}
		}

		public static T GetService<T>() where T : class
		{
			return ServiceProvider.GetService<T>();
		}

		public static IEnumerable<T> GetServices<T>() where T : class
		{
			return ServiceProvider.GetServices<T>();
		}

	}

	internal class Lazier<T> : Lazy<T> where T : class
	{
		public Lazier(IServiceProvider provider)
			: base(() => provider.GetRequiredService<T>())
		{
		}
	}
}
