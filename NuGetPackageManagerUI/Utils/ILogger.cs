using System;

namespace NuGetPackageManagerUI.Utils
{
	public interface ILogger
	{
		event Action<string> OnLog;

		void Log(string message, params object[] args);
	}
}