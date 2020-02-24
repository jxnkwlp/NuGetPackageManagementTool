using System;

namespace NuGetPackageManagerUI.Utils
{
	public class SimpleLogger : ILogger
	{
		public event Action<string> OnLog;

		public void Log(string message, params object[] args)
		{
			OnLog?.Invoke(string.Format(message, args));
		}
	}
}
