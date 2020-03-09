using NuGet.Common;
using System;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.Services.NuGets
{
	public class DefaultNuGetLogger : ILogger
	{
		public event Action<LogLevel, string> OnLog;

		public void Log(LogLevel level, string data)
		{
			OnLog?.Invoke(level, data);
		}

		public void Log(ILogMessage message)
		{
			OnLog?.Invoke(message.Level, message.FormatWithCode());
		}

		public Task LogAsync(LogLevel level, string data)
		{
			OnLog?.Invoke(level, data);
			return Task.CompletedTask;
		}

		public Task LogAsync(ILogMessage message)
		{
			OnLog?.Invoke(message.Level, message.FormatWithCode());
			return Task.CompletedTask;
		}

		public void LogDebug(string data)
		{
			OnLog?.Invoke(LogLevel.Debug, data);
		}

		public void LogError(string data)
		{
			OnLog?.Invoke(LogLevel.Error, data);
		}

		public void LogInformation(string data)
		{
			OnLog?.Invoke(LogLevel.Information, data);
		}

		public void LogInformationSummary(string data)
		{
			OnLog?.Invoke(LogLevel.Information, data);
		}

		public void LogMinimal(string data)
		{
			OnLog?.Invoke(LogLevel.Minimal, data);
		}

		public void LogVerbose(string data)
		{
			OnLog?.Invoke(LogLevel.Verbose, data);
		}

		public void LogWarning(string data)
		{
			OnLog?.Invoke(LogLevel.Warning, data);
		}
	}
}