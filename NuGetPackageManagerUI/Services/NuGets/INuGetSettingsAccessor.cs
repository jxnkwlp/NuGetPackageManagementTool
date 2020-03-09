using NuGet.Configuration;
using System;

namespace NuGetPackageManagerUI.Services.NuGets
{
	public interface INuGetSettingsAccessor : ISingletonService
	{
		Lazy<ISettings> Settings { get; }
	}
}
