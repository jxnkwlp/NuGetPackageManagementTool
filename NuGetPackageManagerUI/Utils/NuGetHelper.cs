using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManagerUI.Utils
{
	public class NuGetHelper
	{
		public static SourceCacheContext CreateSourceCacheContext()
		{
			return new SourceCacheContext();
		}
	}
}
