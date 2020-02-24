using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI
{
	public class ProcessResult
	{
		public bool Success { get; set; }

		public string Text { get; set; }
	}

	internal static class Cmder
	{
		public static async Task<ProcessResult> RunAsync(string file, string workDirectory, string arguments)
		{
			ProcessResult result = new ProcessResult();
			ProcessStartInfo info = new ProcessStartInfo(file)
			{
				WorkingDirectory = workDirectory,
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardError = true,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				Arguments = arguments
			};

			StringBuilder stringBuilder = new StringBuilder();

			Process process = new Process();

			process.OutputDataReceived += (sender, e) =>
			{
				stringBuilder.Append(e.Data);
			};
			process.ErrorDataReceived += (sender, e) =>
			{
				stringBuilder.Append(e.Data);
			};
			process.Exited += delegate
			{
			};

			process.StartInfo = info;
			bool isStart = false;

			try
			{
				isStart = process.Start();
			}
			catch (Exception)
			{
				result.Success = false;
			}

			if (isStart)
			{
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();

				await Task.Run(() =>
				{
					process.WaitForExit();
				});

				result.Success = true;
				result.Text = stringBuilder.ToString();
			}

			return result;
		}
	}
}
