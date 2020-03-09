using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI
{
	public static class ViewModelLocator
	{
		static ViewModelLocator()
		{
		}

		public static TViewModel GetViewModel<TViewModel>() where TViewModel : class => ServiceLocator.GetService<TViewModel>();
	}
}
