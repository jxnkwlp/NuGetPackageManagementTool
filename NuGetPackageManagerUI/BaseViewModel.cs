using NuGetPackageManagerUI.Services;
using NuGetPackageManagerUI.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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

		protected ProjectService ProjectService => ServiceLocator.GetService<ProjectService>();
		protected ILogger Logger => ServiceLocator.GetService<ILogger>();

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

		protected void ValidateProperty<T>(T value, string propertyName)
		{
			Validator.ValidateProperty(value, new ValidationContext(this, null, null)
			{
				MemberName = propertyName
			});
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
