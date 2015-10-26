using NerdAmigo.Abstractions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerdAmigo.Common.Configuration
{
	public class AppConfigEnvironmentConfigurationNameResolver : IEnvironmentConfigurationNameResolver
	{
		public string GetConfigurationName()
		{
			var appSetting = ConfigurationManager.AppSettings["EnvironmentConfigurationName"];
			return appSetting ?? "devConfig";
		}
	}
}
