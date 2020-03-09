using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.Services.NuGets.ProjectServices
{
	public abstract class GlobalProjectServiceProvider
	{
		public virtual T GetGlobalService<T>() where T : class => null;
	}
}
