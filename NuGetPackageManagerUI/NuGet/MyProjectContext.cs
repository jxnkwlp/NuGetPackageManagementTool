using NuGet.Common;
using NuGet.Packaging;
using NuGet.ProjectManagement;
using System;
using System.Xml.Linq;

namespace NuGetPackageManagerUI.NuGet
{
	public class MyProjectContext : INuGetProjectContext
	{
		public PackageExtractionContext PackageExtractionContext
		{
			get;
			set;
		}

		public ISourceControlManagerProvider SourceControlManagerProvider
		{
			get;
		}

		public ExecutionContext ExecutionContext
		{
			get;
		}

		public XDocument OriginalPackagesConfig
		{
			get;
			set;
		}

		public NuGetActionType ActionType
		{
			get;
			set;
		}

		public Guid OperationId
		{
			get;
			set;
		}

		public FileConflictAction FileConflictAction
		{
			get;
			set;
		}

		public MyProjectContext(FileConflictAction fileConflictAction)
		{
			FileConflictAction = fileConflictAction;
		}

		public void Log(MessageLevel level, string message, params object[] args)
		{
		}

		public void Log(ILogMessage message)
		{
		}

		public void ReportError(string message)
		{
		}

		public void ReportError(ILogMessage message)
		{
		}

		public FileConflictAction ResolveFileConflict(string message)
		{
			return FileConflictAction;
		}
	}
}
