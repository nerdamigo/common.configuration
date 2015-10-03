using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NerdAmigo.Abstractions;
using System.IO;

namespace NerdAmigo.Common.Configuration
{
	public class ConfigurationFile : IFileStorableObject<ConfigurationFile>
	{
		private string mPrefix;
		private string mPrefixGroup;
		private Type mConfigObjectType;
		public ConfigurationFile(Type aConfigObjectType, string aPrefixGroup, string aPrefix)
		{
			this.mConfigObjectType = aConfigObjectType;

			if (String.IsNullOrWhiteSpace(aPrefixGroup))
			{
				throw new ArgumentNullException("aPrefixGroup");
			}

			this.mPrefixGroup = aPrefixGroup;
			this.mPrefix = aPrefix;
		}

		public ConfigurationFile(Type aConfigObjectType, string aPrefixGroup) :
			this(aConfigObjectType, aPrefixGroup, null)
		{
		}

		public string FileName
		{
			get
			{
				Type thisType = this.mConfigObjectType;
				string configFileName = String.Format("{0}.{1}.config.xml", thisType.Namespace, thisType.Name);
				string fullRelativePath;
				if (String.IsNullOrEmpty(this.mPrefix))
				{
					fullRelativePath = Path.Combine("config", this.mPrefixGroup, configFileName);
				}
				else
				{
					fullRelativePath = Path.Combine("config", this.mPrefixGroup, this.mPrefix, configFileName);
				}
				return fullRelativePath;
			}
		}
	}
}
