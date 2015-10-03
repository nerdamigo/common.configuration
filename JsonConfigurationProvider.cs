using NerdAmigo.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerdAmigo.Common.Configuration
{
    public class JsonConfigurationProvider<TConfig> : IConfigurationProvider<TConfig> where TConfig : ICloneable, new()
    {
		private IPathMapper PathMapper;
		private Type ConfigType;
		private string ConfigFilePath;
		private const string BasePath = "~/App_Data/Config";
		private TConfig _CurrentConfiguration;
		public JsonConfigurationProvider(IPathMapper PathMapper)
		{
			this.PathMapper = PathMapper;
			this.ConfigType = typeof(TConfig);
			this.ConfigFilePath = PathMapper.MapPath(Path.Combine(BasePath, String.Format("{0}.{1}.config.json", this.ConfigType.Namespace, this.ConfigType.Name)));
			this.CurrentConfiguration = new TConfig();

			ReadConfiguration();
		}

		private void ReadConfiguration()
		{
			FileInfo configFile = new FileInfo(this.ConfigFilePath);
			if (!configFile.Exists)
			{
				return;
			}

			using(FileStream configStream = configFile.OpenRead())
			using(StreamReader configReader = new StreamReader(configStream))
			{
				string configData = configReader.ReadToEnd();
				try
				{
					TConfig newConfig = JsonConvert.DeserializeObject<TConfig>(configData);
					this.CurrentConfiguration = newConfig;
				}
				catch
				{
					this.CurrentConfiguration = new TConfig();
				}
			}
		}

		public void ConfigurationUpdated(Action<TConfig> ConfigurationUpdatedAction)
		{
			//TODO: Implementation
		}

		public TConfig CurrentConfiguration
		{
			get { return (TConfig)this._CurrentConfiguration.Clone(); }
			private set { this._CurrentConfiguration = value; }
		}
	}
}
