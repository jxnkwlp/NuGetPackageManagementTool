using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using NuGet.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace NuGetPackageManagerUI.VisualStudio
{
	public class MSProjectManager
	{
		public Project MSBuildProject { get; private set; }
		public string ProjectFullPath { get; }
		public string ProjectDirectory => Path.GetDirectoryName(ProjectFullPath);
		public string SolutionPath { get; set; }

		public MSProjectManager(string projectFullPath)
		{
			var loadedProjects = ProjectCollection.GlobalProjectCollection.GetLoadedProjects(projectFullPath);
			if (loadedProjects.Any())
			{
				ProjectCollection.GlobalProjectCollection.UnloadProject(loadedProjects.First());
			}

			MSBuildProject = Project.FromFile(projectFullPath, new Microsoft.Build.Definition.ProjectOptions());

			ProjectFullPath = MSBuildProject.FullPath;
		}

		public void CommitChanges()
		{
			MSBuildProject.Save();
		}

		public void CommitChanges(Encoding encoding)
		{
			MSBuildProject.Save(encoding);
		}

		public string GetProjectName()
		{
			return MSBuildProject.GetPropertyValue("ProjectName");
		}

		public string GetProjectTypeGuid()
		{
			return MSBuildProject.GetPropertyValue("DefaultProjectTypeGuid");
		}

		public string GetTargetFramework()
		{
			return MSBuildProject.GetPropertyValue("TargetFramework");
		}

		public string GetTargetFrameworkIdentifier()
		{
			return MSBuildProject.GetPropertyValue("TargetFrameworkIdentifier");
		}
		public string GetTargetFrameworkVersion()
		{
			return MSBuildProject.GetPropertyValue("TargetFrameworkVersion");
		}

		public string GetMSBuildProjectName()
		{
			return MSBuildProject.GetPropertyValue("MSBuildProjectName");
		}

		public string GetNuGetProjectStyle()
		{
			return MSBuildProject.GetPropertyValue("NuGetProjectStyle");
		}

		public void AddItem(string itemType, string unevaluatedInclude, IEnumerable<KeyValuePair<string, string>> metadata = null)
		{
			MSBuildProject.AddItem(itemType, unevaluatedInclude, metadata);
		}

		public ProjectProperty SetProperty(string name, string unevaluatedValue)
		{
			return MSBuildProject.SetProperty(name, unevaluatedValue);
		}

		public ICollection<ProjectItem> GetItems()
		{
			return MSBuildProject.Items;
		}

		public ICollection<ProjectItem> GetItems(string itemType)
		{
			return MSBuildProject.GetItems(itemType);
		}

		public ICollection<ProjectItem> GetItemsByEvaluatedInclude(string evaluatedInclude)
		{
			return MSBuildProject.GetItemsByEvaluatedInclude(evaluatedInclude);
		}

		public ProjectItem GetItem(string itemType, string evaluatedInclude)
		{
			return MSBuildProject.GetItems(itemType).FirstOrDefault(t => t.UnevaluatedInclude == evaluatedInclude);
		}

		public ProjectProperty GetProperty(string name)
		{
			return MSBuildProject.GetProperty(name);
		}

		public string GetPropertyValue(string name)
		{
			return MSBuildProject.GetPropertyValue(name);
		}

		public bool RemoveItem(ProjectItem item)
		{
			return MSBuildProject.RemoveItem(item);
		}

		public void RemoveItems(IEnumerable<ProjectItem> items)
		{
			MSBuildProject.RemoveItems(items);
		}

		public bool RemoveProperty(ProjectProperty property)
		{
			return MSBuildProject.RemoveProperty(property);
		}

		public void AddImport(string targetFullPath, bool top)
		{
			// an doubule backslash
			string targetRelativePath = PathUtility.GetRelativePath(PathUtility.EnsureTrailingSlash(Path.GetDirectoryName(ProjectFullPath)), targetFullPath);

			if (MSBuildProject.Xml.Imports != null)
				if (MSBuildProject.Xml.Imports.Any(t => t.Project.Equals(targetRelativePath, StringComparison.OrdinalIgnoreCase)))
					return;

			var targetElement = MSBuildProject.Xml.AddImport(targetRelativePath);
			targetElement.Condition = "Exists('" + targetRelativePath + "')";

			if (top)
			{
				targetElement.Parent.RemoveChild(targetElement);
				MSBuildProject.Xml.InsertBeforeChild(targetElement, MSBuildProject.Xml.FirstChild);
			}

			AddEnsureImportedTarget(targetRelativePath);
			MSBuildProject.ReevaluateIfNecessary();
		}

		public void RemoveImport(string targetFullPath)
		{
			// an doubule backslash
			string targetRelativePath = PathUtility.GetRelativePath(PathUtility.EnsureTrailingSlash(Path.GetDirectoryName(ProjectFullPath)), targetFullPath);

			foreach (var importElement in MSBuildProject.Xml.Imports)
			{
				if (StringComparer.OrdinalIgnoreCase.Equals(targetRelativePath, importElement.Project))
				{
					importElement.Parent.RemoveChild(importElement);

					RemoveEnsureImportedTarget(targetRelativePath);
					MSBuildProject.ReevaluateIfNecessary();
				}
			}

			//foreach (var importElement in MSBuildProject.Xml.Imports)
			//{
			//	var projectPath = PathUtility.GetPathWithForwardSlashes(importElement.Project);
			//	if (StringComparer.OrdinalIgnoreCase.Equals(targetRelativePath, projectPath))
			//	{
			//		importElement.Parent.RemoveChild(importElement);

			//		RemoveEnsureImportedTarget(targetRelativePath);
			//		MSBuildProject.ReevaluateIfNecessary();
			//	}
			//}
		}

		public IEnumerable<string> GetImportProjects()
		{
			return MSBuildProject.Xml.Imports.Select(t => t.Project).ToArray();
		}

		public void AddEnsureImportedTarget(string targetRelativePath)
		{
			var targetElement = MSBuildProject.Xml.Targets.FirstOrDefault(t => t.Name.Equals("EnsureNuGetPackageBuildImports", StringComparison.OrdinalIgnoreCase));

			if (targetElement == null)
			{
				targetElement = MSBuildProject.Xml.AddTarget("EnsureNuGetPackageBuildImports");
				targetElement.BeforeTargets = "PrepareForBuild";

				var propertyGroup = targetElement.AddPropertyGroup();
				propertyGroup.AddProperty("ErrorText", "This project references NuGet package(s) that are missing on this computer. Enable NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105.The missing file is {0}.");
			}

			// error task
			var errorTask = targetElement.AddTask("Error");
			errorTask.Condition = "!Exists('" + targetRelativePath + "')";
			string errorText = string.Format(CultureInfo.InvariantCulture, "$([System.String]::Format('$(ErrorText)', '{0}'))", targetRelativePath);

			errorTask.SetParameter("Text", errorText);
		}

		public void RemoveEnsureImportedTarget(string targetRelativePath)
		{
			var targetElement = MSBuildProject.Xml.Targets.FirstOrDefault(t => t.Name.Equals("EnsureNuGetPackageBuildImports", StringComparison.OrdinalIgnoreCase));

			if (targetElement == null)
			{
				return;
			}

			string errorCondition = "!Exists('" + PathUtility.GetPathWithForwardSlashes(targetRelativePath) + "')";

			ProjectTaskElement taskElement = null;
			foreach (var task in targetElement.Tasks)
			{
				var currentCondition = PathUtility.GetPathWithForwardSlashes(task.Condition);
				if (string.Equals(currentCondition, errorCondition, StringComparison.OrdinalIgnoreCase))
				{
					taskElement = task;
					break;
				}
			}

			if (taskElement != null)
			{
				taskElement.Parent.RemoveChild(taskElement);

				if (taskElement.Count == 0)
				{
					targetElement.Parent.RemoveChild(targetElement);
				}
			}
		}

	}
}
