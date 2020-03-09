using System.Threading.Tasks;
using System.Windows;

namespace NuGetPackageManagerUI.Services
{
	public class DefaultDialogService : IDialogService
	{
		public Task<bool> ShowConfirmAsync(string message, string title)
		{
			TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

			if (MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
			{
				taskCompletionSource.SetResult(true);
			}
			else
			{
				taskCompletionSource.SetResult(false);
			}

			return taskCompletionSource.Task;
		}

		public Task<bool> ShowErrorAsync(string message, string title)
		{
			TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

			if (MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error) == MessageBoxResult.OK)
			{
				taskCompletionSource.SetResult(true);
			}
			else
			{
				taskCompletionSource.SetResult(false);
			}

			return taskCompletionSource.Task;
		}

		public Task<bool> ShowMessageBoxAsync(string message, string title)
		{
			TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

			if (MessageBox.Show(message, title) == MessageBoxResult.OK)
			{
				taskCompletionSource.SetResult(true);
			}
			else
			{
				taskCompletionSource.SetResult(false);
			}

			return taskCompletionSource.Task;
		}

	}
}
