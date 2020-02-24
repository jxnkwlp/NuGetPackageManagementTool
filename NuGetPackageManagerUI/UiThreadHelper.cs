using System;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI
{
	public class UiThreadHelper
	{
		public static void RunOnUiThread(Func<Task> action)
		{
			var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
			Task.Factory.StartNew(() => action(), default, TaskCreationOptions.LongRunning, uiScheduler);
		}

		public static void RunOnUiThread(Action action)
		{
			var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
			Task.Factory.StartNew(() => action(), default, TaskCreationOptions.LongRunning, uiScheduler);
		}
	}
}
