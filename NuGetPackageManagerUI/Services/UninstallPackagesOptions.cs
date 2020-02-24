namespace NuGetPackageManagerUI.Services
{
	public class UninstallPackagesOptions
	{
		public bool RemoveDependencies { get; set; } = true;
		public bool FocusRemove { get; set; } = true;
	}
}
