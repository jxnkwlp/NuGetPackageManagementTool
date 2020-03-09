using System.Threading.Tasks;

namespace NuGetPackageManagerUI.Services
{
	public interface IDialogService : ITransientService
	{
		Task<bool> ShowMessageBoxAsync(string message, string title);

		Task<bool> ShowConfirmAsync(string message, string title);

		Task<bool> ShowErrorAsync(string message, string title);
	}
}
