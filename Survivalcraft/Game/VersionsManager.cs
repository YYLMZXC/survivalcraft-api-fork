using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Engine;
using Engine.Serialization;
using XmlUtilities;

namespace Game
{
	public static class VersionsManager
	{
		private static List<VersionConverter> m_versionConverters;

		public static Platform Platform => Platform.Windows81;

		public static BuildConfiguration BuildConfiguration => BuildConfiguration.Release;

		public static string Version { get; private set; }

		public static string SerializationVersion { get; private set; }

		public static string LastLaunchedVersion { get; private set; }

		static VersionsManager()
		{
			m_versionConverters = new List<VersionConverter>();
			AssemblyName assemblyName = new AssemblyName(typeof(VersionsManager).GetTypeInfo().Assembly.FullName);
			Version = $"{assemblyName.Version.Major}.{assemblyName.Version.Minor}.{assemblyName.Version.Build}.{assemblyName.Version.Revision}";
			SerializationVersion = string.Format("{0}.{1}", new object[2]
			{
				assemblyName.Version.Major,
				assemblyName.Version.Minor
			});
			Assembly[] array = TypeCache.LoadedAssemblies.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				foreach (TypeInfo definedType in array[i].DefinedTypes)
				{
					if (!definedType.IsAbstract && !definedType.IsInterface && typeof(VersionConverter).GetTypeInfo().IsAssignableFrom(definedType))
					{
						VersionConverter item = (VersionConverter)Activator.CreateInstance(definedType.AsType());
						m_versionConverters.Add(item);
					}
				}
			}
		}

		public static void Initialize()
		{
			LastLaunchedVersion = SettingsManager.LastLaunchedVersion;
			SettingsManager.LastLaunchedVersion = Version;
			if (Version != LastLaunchedVersion)
			{
				AnalyticsManager.LogEvent("[VersionsManager] Upgrade game", new AnalyticsParameter("LastVersion", LastLaunchedVersion), new AnalyticsParameter("CurrentVersion", Version));
			}
		}

		public static void UpgradeProjectXml(XElement projectNode)
		{
			string attributeValue = XmlUtils.GetAttributeValue(projectNode, "Version", "1.0");
			if (!(attributeValue != SerializationVersion))
			{
				return;
			}
			List<VersionConverter> list = FindTransform(attributeValue, SerializationVersion, m_versionConverters, 0);
			if (list == null)
			{
				throw new InvalidOperationException(string.Format("Cannot find conversion path from version \"{0}\" to version \"{1}\"", new object[2] { attributeValue, SerializationVersion }));
			}
			foreach (VersionConverter item in list)
			{
				Log.Information(string.Format("Upgrading world version \"{0}\" to \"{1}\".", new object[2] { item.SourceVersion, item.TargetVersion }));
				item.ConvertProjectXml(projectNode);
			}
			string attributeValue2 = XmlUtils.GetAttributeValue(projectNode, "Version", "1.0");
			if (attributeValue2 != SerializationVersion)
			{
				throw new InvalidOperationException(string.Format("Upgrade produced invalid project version. Expected \"{0}\", found \"{1}\".", new object[2] { SerializationVersion, attributeValue2 }));
			}
		}

		public static void UpgradeWorld(string directoryName)
		{
			WorldInfo worldInfo = WorldsManager.GetWorldInfo(directoryName);
			if (worldInfo == null)
			{
				throw new InvalidOperationException($"Cannot determine version of world at \"{directoryName}\"");
			}
			if (!(worldInfo.SerializationVersion != SerializationVersion))
			{
				return;
			}
			ProgressManager.UpdateProgress($"Upgrading World To {SerializationVersion}", 0f);
			List<VersionConverter> list = FindTransform(worldInfo.SerializationVersion, SerializationVersion, m_versionConverters, 0);
			if (list == null)
			{
				throw new InvalidOperationException(string.Format("Cannot find conversion path from version \"{0}\" to version \"{1}\"", new object[2] { worldInfo.SerializationVersion, SerializationVersion }));
			}
			foreach (VersionConverter item in list)
			{
				Log.Information(string.Format("Upgrading world version \"{0}\" to \"{1}\".", new object[2] { item.SourceVersion, item.TargetVersion }));
				item.ConvertWorld(directoryName);
			}
			WorldInfo worldInfo2 = WorldsManager.GetWorldInfo(directoryName);
			if (worldInfo2.SerializationVersion != SerializationVersion)
			{
				throw new InvalidOperationException(string.Format("Upgrade produced invalid project version. Expected \"{0}\", found \"{1}\".", new object[2] { SerializationVersion, worldInfo2.SerializationVersion }));
			}
			AnalyticsManager.LogEvent("[VersionConverter] Upgrade world", new AnalyticsParameter("SourceVersion", worldInfo.SerializationVersion), new AnalyticsParameter("TargetVersion", SerializationVersion));
		}

		public static int CompareVersions(string v1, string v2)
		{
			string[] array = v1.Split('.');
			string[] array2 = v2.Split('.');
			for (int i = 0; i < MathUtils.Min(array.Length, array2.Length); i++)
			{
				int result;
				int result2;
				int num = ((!int.TryParse(array[i], out result) || !int.TryParse(array2[i], out result2)) ? string.CompareOrdinal(array[i], array2[i]) : (result - result2));
				if (num != 0)
				{
					return num;
				}
			}
			return array.Length - array2.Length;
		}

		private static List<VersionConverter> FindTransform(string sourceVersion, string targetVersion, IEnumerable<VersionConverter> converters, int depth)
		{
			if (depth > 100)
			{
				throw new InvalidOperationException("Too deep recursion when searching for version converters. Check for possible loops in transforms.");
			}
			if (sourceVersion == targetVersion)
			{
				return new List<VersionConverter>();
			}
			List<VersionConverter> result = null;
			int num = int.MaxValue;
			foreach (VersionConverter converter in converters)
			{
				if (converter.SourceVersion == sourceVersion)
				{
					List<VersionConverter> list = FindTransform(converter.TargetVersion, targetVersion, converters, depth + 1);
					if (list != null && list.Count < num)
					{
						num = list.Count;
						list.Insert(0, converter);
						result = list;
					}
				}
			}
			return result;
		}
	}
}
