using NerdAmigo.Abstractions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerdAmigo.Common.Configuration
{
	public class AppConfigEnvironmentNameResolver : IEnvironmentNameResolver
	{
		public string GetEnvironmentName()
		{
			var appSetting = ConfigurationManager.AppSettings["EnvironmentName"];
			return appSetting;
		}
	}
}
