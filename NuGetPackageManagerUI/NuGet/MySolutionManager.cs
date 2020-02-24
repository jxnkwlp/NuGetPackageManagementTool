using NuGet.Configuration;
using NuGet.PackageManagement;
using NuGet.ProjectManagement;
using NuGetPackageManagerUI.Models;
using NuGetPackageManagerUI.VisualStudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.NuGet
{
	internal class MySolutionManager : ISolutionManager
	{
		private IEnumerable<ProjectModel> _projects;

		public ISettings NuGetSettings { get; }

		public IEnumerable<NuGetProject> NuGetProjects { get; private set; }

		public string PackagesFolderPath { get; }

		public string MsbuildDirectory { get; }

		public string SolutionDirectory { get; }

		public bool IsSolutionOpen => false;

		public INuGetProjectContext NuGetProjectContext { get; set; }

		#region event

		public event EventHandler SolutionOpening;

		public event EventHandler SolutionOpened;

		public event EventHandler SolutionClosing;

		public event EventHandler SolutionClosed;

		public event EventHandler<NuGetEventArgs<string>> AfterNuGetCacheUpdated;

		public event EventHandler<NuGetProjectEventArgs> NuGetProjectAdded;

		public event EventHandler<NuGetProjectEventArgs> NuGetProjectRemoved;

		public event EventHandler<NuGetProjectEventArgs> NuGetProjectRenamed;

		public event EventHandler<NuGetProjectEventArgs> NuGetProjectUpdated;

		public event EventHandler<NuGetProjectEventArgs> AfterNuGetProjectRenamed;

		public event EventHandler<ActionsExecutedEventArgs> ActionsExecuted;

		#endregion event

		public MySolutionManager(string solutionDirectory, string msbuildDirectory) : this(solutionDirectory, msbuildDirectory, Enumerable.Empty<ProjectModel>())
		{
		}

		public MySolutionManager(string solutionDirectory, string msbuildDirectory, IEnumerable<ProjectModel> projects)
		{
			SolutionDirectory = (solutionDirectory ?? throw new ArgumentNullException("solutionDirectory"));
			MsbuildDirectory = msbuildDirectory;
			_projects = projects;

			NuGetSettings = Settings.LoadDefaultSettings(solutionDirectory);

			PackagesFolderPath = PackagesFolderPathUtility.GetPackagesFolderPath(this, NuGetSettings);

			NuGetProjects = projects.Select(t => CreateProject(t)).ToList();
		}

		public void UpdateProjects(IEnumerable<ProjectModel> projects)
		{
			if (projects is null)
			{
				throw new ArgumentNullException(nameof(projects));
			}

			_projects = projects;

			EnsureSolutionIsLoaded();
		}

		public Task<bool> DoesNuGetSupportsAnyProjectAsync()
		{
			return Task.FromResult(result: true);
		}

		public void EnsureSolutionIsLoaded()
		{
			// reload
			NuGetProjects = _projects.Select(t => CreateProject(t)).ToList();
		}

		public Task<NuGetProject> GetNuGetProjectAsync(string nuGetProjectSafeName)
		{
			var nuGetProject = NuGetProjects.Where(t => t != null && t.Metadata != null).FirstOrDefault(t => t.Metadata["Name"].ToString() == nuGetProjectSafeName);

			if (nuGetProject == null)
				throw new Exception($"Project {nuGetProjectSafeName} is unsupport.");

			return Task.FromResult(nuGetProject);
		}

		public async Task<string> GetNuGetProjectSafeNameAsync(NuGetProject nuGetProject)
		{
			string name = nuGetProject.GetMetadata<string>("Name");
			if (await GetNuGetProjectAsync(name) == nuGetProject)
			{
				return name;
			}
			return NuGetProject.GetUniqueNameOrName(nuGetProject);
		}

		public Task<IEnumerable<NuGetProject>> GetNuGetProjectsAsync()
		{
			return Task.FromResult(NuGetProjects ?? Enumerable.Empty<NuGetProject>());
		}

		public Task<bool> IsSolutionAvailableAsync()
		{
			return Task.FromResult(result: true);
		}

		public void OnActionsExecuted(IEnumerable<ResolvedAction> actions)
		{
			ActionsExecuted?.Invoke(this, new ActionsExecutedEventArgs(actions));
		}

		protected NuGetProject CreateProject(ProjectModel project)
		{
			INuGetProjectContext context = NuGetProjectContext ?? new EmptyNuGetProjectContext();
			//MSBuildProjectSystem projectSystem = new MSBuildProjectSystem(MsbuildDirectory, project.FullPath, NuGetProjectContext);
			//VsMSBuildProjectSystem projectSystem = new VsMSBuildProjectSystem(project.FullPath, NuGetProjectContext);

			//return new MSBuildNuGetProject(projectSystem, PackagesFolderPath, project.FolderPath);

			var projectManager = new MSProjectManager(project.FullPath);
			return NuGetProjectFactory.CreateNuGetProject(this, projectManager, context);
		}
	}
}
