using NuGet.Configuration;
using System;

namespace NuGetPackageManagerUI.Services.NuGets
{
	public class NuGetSettingsAccessor : INuGetSettingsAccessor
	{
		private readonly ISolutionDiretoryManager _solutionDiretoryManager;

		private string _diretoryPath = null;
		private ISettings _settings;

		public Lazy<ISettings> Settings => new Lazy<ISettings>(() =>
		{
			var newDirectory = _solutionDiretoryManager.DiretoryPath;
			if (newDirectory != _diretoryPath || _settings == null)
			{
				_diretoryPath = newDirectory;
				_settings = NuGet.Configuration.Settings.LoadDefaultSettings(newDirectory);
			}
			return _settings;
		}, false);

		public NuGetSettingsAccessor(ISolutionDiretoryManager solutionDiretoryManager)
		{
			_solutionDiretoryManager = solutionDiretoryManager;
		}
	}
}
