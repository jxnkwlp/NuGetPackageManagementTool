using System;
using TinyIoC;

namespace NuGetPackageManagerUI
{
	public class ServiceLocator
	{
		public static void RegisterService<TService>() where TService : class
		{
			TinyIoCContainer.Current.Register<TService>();
		}

		public static void RegisterService<TService, TImpl>() where TService : class where TImpl : class, TService
		{
			TinyIoCContainer.Current.Register<TService, TImpl>(); 
		}

		public static void RegisterService<TService>(Func<TService> impl) where TService : class
		{
			TinyIoCContainer.Current.Register((_, __) => impl());
		}

		public static void RegisterService<TService>(TService instance) where TService : class
		{
			TinyIoCContainer.Current.Register(instance);
		}

		public static TService GetService<TService>() where TService : class
		{
			return TinyIoCContainer.Current.Resolve<TService>();
		}
	}
}
