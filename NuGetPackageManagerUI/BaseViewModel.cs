using NuGet.PackageManagement;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGetPackageManagerUI.MsBuild;
using NuGetPackageManagerUI.Services;
using NuGetPackageManagerUI.Services.NuGets;
using NuGetPackageManagerUI.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI
{
	public class BaseViewModel : INotifyPropertyChanged
	{
		private bool _isBusy;
		private string _title;

		public bool IsBusy
		{
			get
			{
				return _isBusy;
			}
			set
			{
				Set(ref _isBusy, value, nameof(IsBusy));
			}
		}

		public string Title { get => _title; set => Set(ref _title, value); }

		#region services
		protected ILogger Logger => ServiceLocator.GetService<ILogger>();
		protected IDialogService DialogService => ServiceLocator.GetService<IDialogService>();
		protected IProjectService ProjectService => ServiceLocator.GetService<IProjectService>();
		protected ISolutionDiretoryManager SolutionDiretoryManager => ServiceLocator.GetService<ISolutionDiretoryManager>();
		protected INuGetPackageService NuGetPackageService => ServiceLocator.GetService<INuGetPackageService>();
		protected ISolutionManagerProvider SolutionManagerProvider => ServiceLocator.GetService<ISolutionManagerProvider>();
		protected IProjectAdapterProvider ProjectAdapterProvider => ServiceLocator.GetService<IProjectAdapterProvider>();


		protected ISolutionManager SolutionManager => SolutionManagerProvider.CreateOrGetSolutionManager();

		#endregion

		public Action CloseWindowAction { get; set; }


		public event PropertyChangedEventHandler PropertyChanged;

		protected bool Set<T>(ref T field, T newValue = default(T), [CallerMemberName] string propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(field, newValue))
			{
				return false;
			}
			field = newValue;
			OnPropertyChanged(propertyName);
			return true;
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		//protected void ValidateProperty<T>(T value, string propertyName)
		//{
		//	Validator.ValidateProperty(value, new ValidationContext(this, null, null)
		//	{
		//		MemberName = propertyName
		//	});
		//}

		protected async Task<Dictionary<NuGetProject, IEnumerable<PackageIdentity>>> GetInstalledPackagesFromProjectFilesAsync2(IEnumerable<string> projectFiles)
		{
			var result = new Dictionary<NuGetProject, IEnumerable<PackageIdentity>>();
			foreach (var item in projectFiles)
			{
				var nuGetProject = await SolutionManager.GetNuGetProjectAsync(item);
				var installedPackages = await nuGetProject.GetInstalledPackagesAsync(default);

				result[nuGetProject] = installedPackages.Select(t => t.PackageIdentity).ToArray();
			}

			return result;
		}

		protected async Task<IEnumerable<PackageIdentity>> GetInstalledPackagesFromProjectFilesAsync(IEnumerable<string> projectFiles)
		{
			var result = new List<PackageIdentity>();
			foreach (var item in projectFiles)
			{
				var nuGetProject = await SolutionManager.GetNuGetProjectAsync(item);
				var installedPackages = await nuGetProject.GetInstalledPackagesAsync(default);

				result.AddRange(installedPackages.Select(t => t.PackageIdentity));
			}

			return result;
		}

		protected async Task<IEnumerable<PackageIdentity>> GetInstalledPackagesFromProjectFileAsync(string projectFile)
		{
			var nuGetProject = await SolutionManager.GetNuGetProjectAsync(projectFile);
			var installedPackages = await nuGetProject.GetInstalledPackagesAsync(default);
			return installedPackages.Select(t => t.PackageIdentity).ToArray();
		}

		public virtual Task InitializeAsync()
		{
			return Task.CompletedTask;
		}

		public virtual void OnWindowsClosing()
		{
		}

		public virtual void CloseWindow()
		{
			CloseWindowAction?.Invoke();
		}
	}
}
