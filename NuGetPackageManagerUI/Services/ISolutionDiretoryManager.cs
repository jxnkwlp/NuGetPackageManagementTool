using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.Services
{
	public interface ISolutionDiretoryManager : ISingletonService
	{
		string DiretoryPath { get; }

		void OpenDiretory(string path);

		// Task<IEnumerable<ProjectModel>> GetProjectsAsync(bool focusLoad = false);

		/// <summary>
		///  Get or find all project files from directory path.
		/// </summary> 
		Task<IEnumerable<string>> GetProjectFilesAsync(bool focus = false);
	}
}
