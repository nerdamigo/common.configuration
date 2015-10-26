using NerdAmigo.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Web.XmlTransform;

namespace NerdAmigo.Common.Configuration
{
	public class XmlConfigurationProvider<TConfig> : IDisposable, IConfigurationProvider<TConfig> where TConfig : class, ICloneable, new()
    {
		private Type mConfigType;
		private TConfig _CurrentConfiguration;
		private ConfigurationFile mConfigFileBase;
		private ConfigurationFile mConfigFileEnvironment;
		private ConfigurationFile mConfigFileHostName;
		private IFileStorageItemInfo<ConfigurationFile> mBaseStorageItem;
		private IFileStorageItemInfo<ConfigurationFile> mEnvironmentStorageItem;
		private IFileStorageItemInfo<ConfigurationFile> mHostStorageItem;
		private HashSet<Action<TConfig>> mUpdateActions;

		//private string ConfigFilePath;
		//private const string BasePath = "~/App_Data/Config";
		public XmlConfigurationProvider(IFileStorageProvider<ConfigurationFile> aStorageProvider,
			IEnvironmentConfigurationNameResolver aEnvironmentNameResolver,
			IHostNameResolver aHostNameResolver)
		{
			this.mConfigType = typeof(TConfig);
			this.mUpdateActions = new HashSet<Action<TConfig>>();
			
			//construct an object for the base, the environment, and the host
			string tEnvironmentName = aEnvironmentNameResolver.GetConfigurationName();
			string tHostName = aHostNameResolver.GetHostName();

			this.mConfigFileBase = new ConfigurationFile(this.mConfigType, "baseConfiguration");
			this.mConfigFileEnvironment = new ConfigurationFile(this.mConfigType, "environmentConfiguration", tEnvironmentName);
			this.mConfigFileHostName = new ConfigurationFile(this.mConfigType, "hostConfiguration", tHostName);

			this.mBaseStorageItem = aStorageProvider.GetStorageItemInfo(this.mConfigFileBase);
			this.mEnvironmentStorageItem = aStorageProvider.GetStorageItemInfo(this.mConfigFileEnvironment);
			this.mHostStorageItem = aStorageProvider.GetStorageItemInfo(this.mConfigFileHostName);
			
			ReadConfiguration();

			this.mBaseStorageItem.OnUpdate(ReadConfiguration);
			this.mEnvironmentStorageItem.OnUpdate(ReadConfiguration);
			this.mHostStorageItem.OnUpdate(ReadConfiguration);
		}

		private void ReadConfiguration()
		{
			string baseConfigData = ReadConfigFile(this.mBaseStorageItem);
			string envConfigData = ReadConfigFile(this.mEnvironmentStorageItem);
			string hostConfigData = ReadConfigFile(this.mHostStorageItem);

			if (String.IsNullOrEmpty(baseConfigData))
			{
				throw new Exception(String.Format("Base configuration data is missing for type '{0}'", this.mConfigType.Name));
			}

			using (XmlTransformableDocument baseDocument = new XmlTransformableDocument())
			{
				baseDocument.PreserveWhitespace = true;
				baseDocument.LoadXml(baseConfigData);

				if (!String.IsNullOrEmpty(envConfigData))
				{
					ApplyTransformation(baseDocument, envConfigData);
				}

				if (!String.IsNullOrEmpty(hostConfigData))
				{
					ApplyTransformation(baseDocument, hostConfigData);
				}


				using(MemoryStream saveStream = new MemoryStream())
				{
					baseDocument.Save(saveStream);
					saveStream.Seek(0, SeekOrigin.Begin);

					try
					{
						XmlSerializer tSerializer = new XmlSerializer(typeof(TConfig));
						TConfig newConfig = (TConfig)tSerializer.Deserialize(saveStream);
						ConfigurationUpdated(newConfig);
					}
					catch(Exception ex)
					{
						throw new Exception(String.Format("Failed to load transformed configuration data for type '{0}'", this.mConfigType.Name), ex);
					}
				}
			}
		}

		private void ApplyTransformation(XmlTransformableDocument document, string hostConfigData)
		{
			using(XmlTransformation transformation = new XmlTransformation(hostConfigData, false, null))
			{
				transformation.Apply(document);
			}
		}

		private string ReadConfigFile(IFileStorageItemInfo<ConfigurationFile> aFileItem)
		{
			if(!aFileItem.Exists())
			{
				return null;				
			}

			using(Stream configStream = aFileItem.Open())
			using(StreamReader configReader = new StreamReader(configStream))
			{
				return configReader.ReadToEnd();
			}
		}

		public void ConfigurationUpdated(Action<TConfig> ConfigurationUpdatedAction)
		{
			if (!this.mUpdateActions.Contains(ConfigurationUpdatedAction))
			{
				this.mUpdateActions.Add(ConfigurationUpdatedAction);
			}
		}

		private void ConfigurationUpdated(TConfig newConfiguration)
		{
			if (newConfiguration != null)
			{
				if (
					this._CurrentConfiguration == null ||
					(this._CurrentConfiguration != null && !this._CurrentConfiguration.Equals(newConfiguration))
				)
				{
					this._CurrentConfiguration = newConfiguration;

					foreach (Action<TConfig> updateAction in this.mUpdateActions)
					{
						updateAction(newConfiguration);
					}
				}
			}
		}

		public TConfig CurrentConfiguration
		{
			get { return (TConfig)this._CurrentConfiguration.Clone(); }
			private set { this._CurrentConfiguration = value; }
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (this.mBaseStorageItem != null)
				{
					this.mBaseStorageItem.Dispose();
				}
				if (this.mEnvironmentStorageItem != null)
				{
					this.mEnvironmentStorageItem.Dispose();
				}
				if (this.mHostStorageItem != null)
				{
					this.mHostStorageItem.Dispose();
				}
			}
		}
	}
}
