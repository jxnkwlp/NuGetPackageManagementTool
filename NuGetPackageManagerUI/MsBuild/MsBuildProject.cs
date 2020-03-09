using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using NuGet.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace NuGetPackageManagerUI.MsBuild
{
	public class MsBuildProject
	{
		//public string SolutionPath { get; set; }
		public string ProjectFilePath { get; }
		public string ProjectDirectory => Path.GetDirectoryName(ProjectFilePath);

		internal Project Project { get; }

		public MsBuildProject(string projectFilePath)
		{
			var loadedProjects = ProjectCollection.GlobalProjectCollection.GetLoadedProjects(projectFilePath);
			if (loadedProjects.Any())
			{
				ProjectCollection.GlobalProjectCollection.UnloadProject(loadedProjects.First());
			}

			Project = Project.FromFile(projectFilePath, new Microsoft.Build.Definition.ProjectOptions());

			ProjectFilePath = Project.FullPath;
		}

		public void CommitChanges()
		{
			Project.Save();
		}

		public void CommitChanges(Encoding encoding)
		{
			Project.Save(encoding);
		}

		public string GetProjectName()
		{
			return Project.GetPropertyValue("ProjectName");
		}

		public string GetProjectUniqueName()
		{
			return ProjectFilePath;
		}

		public string GetProjectTypeGuids()
		{
			return Project.GetPropertyValue("ProjectTypeGuids");
		}

		public string GetProjectGuid()
		{
			return Project.GetPropertyValue("ProjectGuid");
		}

		public string GetTargetFramework()
		{
			return Project.GetPropertyValue("TargetFramework");
		}

		public string GetTargetFrameworkIdentifier()
		{
			return Project.GetPropertyValue("TargetFrameworkIdentifier");
		}
		public string GetTargetFrameworkVersion()
		{
			return Project.GetPropertyValue("TargetFrameworkVersion");
		}

		public string GetMSBuildProjectName()
		{
			return Project.GetPropertyValue("MSBuildProjectName");
		}

		public string GetNuGetProjectStyle()
		{
			return Project.GetPropertyValue("NuGetProjectStyle");
		}

		public string GetMSBuildProjectExtensionsPath()
		{
			return Project.GetPropertyValue("MSBuildProjectExtensionsPath");
		}

		public void AddItem(string itemType, string unevaluatedInclude, IEnumerable<KeyValuePair<string, string>> metadata = null)
		{
			Project.AddItem(itemType, unevaluatedInclude, metadata);
		}

		public void AddItemMetadata(ProjectItem projectItem, string name, string value, bool expressAsAttribute)
		{
			projectItem.Xml.AddMetadata(name, value, expressAsAttribute);
		}

		public void SetMetadataValue(ProjectItem projectItem, string name, string value)
		{
			projectItem.SetMetadataValue(name, value);
		}

		public bool RemoveMetadata(ProjectItem projectItem, string name)
		{
			return projectItem.RemoveMetadata(name);
		}

		public IEnumerable<ProjectMetadata> GetMetadatas(ProjectItem projectItem)
		{
			return projectItem.Metadata;
		}

		public ProjectProperty SetProperty(string name, string unevaluatedValue)
		{
			return Project.SetProperty(name, unevaluatedValue);
		}

		public ICollection<ProjectItem> GetItems()
		{
			return Project.Items;
		}

		public ICollection<ProjectItem> GetItems(string itemType)
		{
			return Project.GetItems(itemType);
		}

		public ICollection<ProjectItem> GetItemsByEvaluatedInclude(string evaluatedInclude)
		{
			return Project.GetItemsByEvaluatedInclude(evaluatedInclude);
		}

		public ProjectItem GetItem(string itemType, string evaluatedInclude)
		{
			return Project.GetItems(itemType).FirstOrDefault(t => t.UnevaluatedInclude == evaluatedInclude);
		}

		public ProjectProperty GetProperty(string name)
		{
			return Project.GetProperty(name);
		}

		public string GetPropertyValue(string name)
		{
			return Project.GetPropertyValue(name);
		}

		public bool RemoveItem(ProjectItem item)
		{
			return Project.RemoveItem(item);
		}

		public void RemoveItems(IEnumerable<ProjectItem> items)
		{
			Project.RemoveItems(items);
		}

		public bool RemoveProperty(ProjectProperty property)
		{
			return Project.RemoveProperty(property);
		}

		public void AddImport(string targetFullPath, bool top)
		{
			// an doubule backslash
			string targetRelativePath = PathUtility.GetRelativePath(PathUtility.EnsureTrailingSlash(Path.GetDirectoryName(ProjectFilePath)), targetFullPath);

			if (Project.Xml.Imports != null)
				if (Project.Xml.Imports.Any(t => t.Project.Equals(targetRelativePath, StringComparison.OrdinalIgnoreCase)))
					return;

			var targetElement = Project.Xml.AddImport(targetRelativePath);
			targetElement.Condition = "Exists('" + targetRelativePath + "')";

			if (top)
			{
				targetElement.Parent.RemoveChild(targetElement);
				Project.Xml.InsertBeforeChild(targetElement, Project.Xml.FirstChild);
			}

			AddEnsureImportedTarget(targetRelativePath);
			Project.ReevaluateIfNecessary();
		}

		public void RemoveImport(string targetFullPath)
		{
			// an doubule backslash
			string targetRelativePath = PathUtility.GetRelativePath(PathUtility.EnsureTrailingSlash(Path.GetDirectoryName(ProjectFilePath)), targetFullPath);

			foreach (var importElement in Project.Xml.Imports)
			{
				if (StringComparer.OrdinalIgnoreCase.Equals(targetRelativePath, importElement.Project))
				{
					importElement.Parent.RemoveChild(importElement);

					RemoveEnsureImportedTarget(targetRelativePath);
					Project.ReevaluateIfNecessary();
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
			return Project.Xml.Imports.Select(t => t.Project).ToArray();
		}

		public void AddEnsureImportedTarget(string targetRelativePath)
		{
			var targetElement = Project.Xml.Targets.FirstOrDefault(t => t.Name.Equals("EnsureNuGetPackageBuildImports", StringComparison.OrdinalIgnoreCase));

			if (targetElement == null)
			{
				targetElement = Project.Xml.AddTarget("EnsureNuGetPackageBuildImports");
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
			var targetElement = Project.Xml.Targets.FirstOrDefault(t => t.Name.Equals("EnsureNuGetPackageBuildImports", StringComparison.OrdinalIgnoreCase));

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
