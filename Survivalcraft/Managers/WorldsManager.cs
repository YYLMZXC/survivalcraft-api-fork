﻿using Engine;
using Engine.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TemplatesDatabase;
using XmlUtilities;

namespace Game
{
	public static class WorldsManager
	{
		public static List<WorldInfo> m_worldInfos = [];

		public static ReadOnlyList<string> m_newWorldNames;

		public static string WorldsDirectoryName = ModsManager.WorldsDirectoryName;

		public const int MaxAllowedWorlds = 30;

		public static ReadOnlyList<string> NewWorldNames => m_newWorldNames;

		public static ReadOnlyList<WorldInfo> WorldInfos => new(m_worldInfos);

		public static event Action<string> WorldDeleted;
		public static bool Loaded = false;

		public static void Initialize()
		{
			if (Loaded) return;
			Storage.CreateDirectory(WorldsDirectoryName);
			string text = ContentManager.Get<string>("NewWorldNames");
			m_newWorldNames = new ReadOnlyList<string>(text.Split(new char[2]
			{
				'\n',
				'\r'
			}, StringSplitOptions.RemoveEmptyEntries));
			Loaded = true;
		}

		public static string ImportWorld(Stream sourceStream)
		{
			if (MarketplaceManager.IsTrialMode)
			{
				throw new InvalidOperationException("Cannot import worlds in trial mode.");
			}
			string unusedWorldDirectoryName = GetUnusedWorldDirectoryName();
			Storage.CreateDirectory(unusedWorldDirectoryName);
			UnpackWorld(unusedWorldDirectoryName, sourceStream, importEmbeddedExternalContent: true);
			if (!TestXmlFile(Storage.CombinePaths(unusedWorldDirectoryName, "Project.xml"), "Project"))
			{
				try
				{
					DeleteWorld(unusedWorldDirectoryName);
				}
				catch
				{
				}
				throw new InvalidOperationException("Cannot import world because it does not contain valid world data.");
			}
			return unusedWorldDirectoryName;
		}

		public static void ExportWorld(string directoryName, Stream targetStream)
		{
			PackWorld(directoryName, targetStream, null, embedExternalContent: true);
		}

		public static void DeleteWorld(string directoryName)
		{
			if (Storage.DirectoryExists(directoryName))
			{
				DeleteWorldContents(directoryName, null);
				Storage.DeleteDirectory(directoryName);
			}
			WorldDeleted?.Invoke(directoryName);
		}

		public static void RepairWorldIfNeeded(string directoryName)
		{
			try
			{
				string text = Storage.CombinePaths(directoryName, "Project.xml");
				if (!TestXmlFile(text, "Project"))
				{
					Log.Warning($"Project file at \"{text}\" is corrupt or nonexistent. Will try copying data from the backup file. If that fails, will try making a recovery project file.");
					string text2 = Storage.CombinePaths(directoryName, "Project.bak");
					if (TestXmlFile(text2, "Project"))
					{
						Storage.CopyFile(text2, text);
					}
					else
					{
						string path = Storage.CombinePaths(directoryName, "Chunks.dat");
						if (!Storage.FileExists(path))
						{
							throw new InvalidOperationException("Recovery project file could not be generated because chunks file does not exist.");
						}
						XElement xElement = ContentManager.Get<XElement>("RecoveryProject");
						using (Stream stream = Storage.OpenFile(path, OpenFileMode.Read))
						{
							TerrainSerializer14.ReadTOCEntry(stream, out int cx, out int cz, out int _);
							var vector = new Vector3(16 * cx, 255f, 16 * cz);
							xElement.Element("Subsystems").Element("Values").Element("Value")
								.Attribute("Value")
								.SetValue(HumanReadableConverter.ConvertToString(vector));
						}
						using (Stream stream2 = Storage.OpenFile(text, OpenFileMode.Create))
						{
							XmlUtils.SaveXmlToStream(xElement, stream2, null, throwOnError: true);
						}
					}
				}
			}
			catch (Exception)
			{
				throw new InvalidOperationException("The world files are corrupt and could not be repaired.");
			}
		}

		public static void MakeQuickWorldBackup(string directoryName)
		{
			string text = Storage.CombinePaths(directoryName, "Project.xml");
			if (Storage.FileExists(text))
			{
				string destinationPath = Storage.CombinePaths(directoryName, "Project.bak");
				Storage.CopyFile(text, destinationPath);
			}
		}

		public static bool SnapshotExists(string directoryName, string snapshotName)
		{
			return Storage.FileExists(MakeSnapshotFilename(directoryName, snapshotName));
		}

		public static void TakeWorldSnapshot(string directoryName, string snapshotName)
		{
			using (Stream targetStream = Storage.OpenFile(MakeSnapshotFilename(directoryName, snapshotName), OpenFileMode.Create))
			{
				PackWorld(directoryName, targetStream, (string fn) => Path.GetExtension(fn).ToLower() != ".snapshot", embedExternalContent: false);
			}
		}

		public static void RestoreWorldFromSnapshot(string directoryName, string snapshotName)
		{
			if (SnapshotExists(directoryName, snapshotName))
			{
				DeleteWorldContents(directoryName, (string fn) => Storage.GetExtension(fn).ToLower() != ".snapshot");
				using (Stream sourceStream = Storage.OpenFile(MakeSnapshotFilename(directoryName, snapshotName), OpenFileMode.Read))
				{
					UnpackWorld(directoryName, sourceStream, importEmbeddedExternalContent: false);
				}
			}
		}

		public static void DeleteWorldSnapshot(string directoryName, string snapshotName)
		{
			string path = MakeSnapshotFilename(directoryName, snapshotName);
			if (Storage.FileExists(path))
			{
				Storage.DeleteFile(path);
			}
		}

		public static void UpdateWorldsList()
		{
			m_worldInfos.Clear();
			foreach (string item in Storage.ListDirectoryNames(WorldsDirectoryName))
			{
				WorldInfo worldInfo = GetWorldInfo(Storage.CombinePaths(WorldsDirectoryName, item));
				if (worldInfo != null)
				{
					m_worldInfos.Add(worldInfo);
				}
			}
		}

		public static bool ValidateWorldName(string name)
		{
			if (name.Contains("\\") || name.Length > 128) return false;
			return true;
		}

		public static WorldInfo GetWorldInfo(string directoryName)
		{
			var worldInfo = new WorldInfo();
			worldInfo.DirectoryName = directoryName;
			worldInfo.LastSaveTime = DateTime.MinValue;
			var list = new List<string>();
			RecursiveEnumerateDirectory(directoryName, list, null, null);
			if (list.Count > 0)
			{
				foreach (string item in list)
				{
					DateTime fileLastWriteTime = Storage.GetFileLastWriteTime(item);
					if (fileLastWriteTime > worldInfo.LastSaveTime)
					{
						worldInfo.LastSaveTime = fileLastWriteTime;
					}
					try
					{
						worldInfo.Size += Storage.GetFileSize(item);
					}
					catch (Exception e2)
					{
						Log.Error(ExceptionManager.MakeFullErrorMessage($"Error getting size of file \"{item}\".", e2));
					}
				}
				string text = Storage.CombinePaths(directoryName, "Project.xml");
				try
				{
					if (Storage.FileExists(text))
					{
						using (Stream stream = Storage.OpenFile(text, OpenFileMode.Read))
						{
							XElement xElement = XmlUtils.LoadXmlFromStream(stream, null, throwOnError: true);
							worldInfo.SerializationVersion = XmlUtils.GetAttributeValue(xElement, "Version", "1.0");
							worldInfo.APIVersion = XmlUtils.GetAttributeValue(xElement, "APIVersion", String.Empty);
							VersionsManager.UpgradeProjectXml(xElement);
							XElement gameInfoNode = GetGameInfoNode(xElement);
							var valuesDictionary = new ValuesDictionary();
							valuesDictionary.ApplyOverrides(gameInfoNode);
							worldInfo.WorldSettings.Load(valuesDictionary);
							foreach (XElement item2 in (from e in GetPlayersNode(xElement).Elements()
														where XmlUtils.GetAttributeValue<string>(e, "Name") == "Players"
														select e).First().Elements())
							{
								var playerInfo = new PlayerInfo();
								worldInfo.PlayerInfos.Add(playerInfo);
								XElement xElement2 = (from e in item2.Elements()
													  where XmlUtils.GetAttributeValue(e, "Name", string.Empty) == "CharacterSkinName"
													  select e).FirstOrDefault();
								if (xElement2 != null)
								{
									playerInfo.CharacterSkinName = XmlUtils.GetAttributeValue(xElement2, "Value", string.Empty);
								}
							}
							return worldInfo;
						}
					}
					return worldInfo;
				}
				catch (Exception e3)
				{
					Log.Error(ExceptionManager.MakeFullErrorMessage($"Error getting data from project file \"{text}\".", e3));
					return worldInfo;
				}
			}
			return null;
		}

		public static WorldInfo CreateWorld(WorldSettings worldSettings)
		{
			string unusedWorldDirectoryName = GetUnusedWorldDirectoryName();
			Storage.CreateDirectory(unusedWorldDirectoryName);
			if (!ValidateWorldName(worldSettings.Name))
			{
				throw new InvalidOperationException($"World name \"{worldSettings.Name}\" is invalid.");
			}
			int num;
			if (string.IsNullOrEmpty(worldSettings.Seed))
			{
				num = (int)(long)(Time.RealTime * 1000.0);
			}
			else if (worldSettings.Seed == "0")
			{
				num = 0;
			}
			else
			{
				num = 0;
				int num2 = 1;
				string seed = worldSettings.Seed;
				foreach (char c in seed)
				{
					num += c * num2;
					num2 += 29;
				}
			}
			var valuesDictionary = new ValuesDictionary();
			worldSettings.Save(valuesDictionary, liveModifiableParametersOnly: false);
			valuesDictionary.SetValue("WorldDirectoryName", unusedWorldDirectoryName);
			valuesDictionary.SetValue("WorldSeed", num);
			var valuesDictionary2 = new ValuesDictionary();
			valuesDictionary2.SetValue("Players", new ValuesDictionary());
			DatabaseObject databaseObject = DatabaseManager.GameDatabase.Database.FindDatabaseObject("GameProject", DatabaseManager.GameDatabase.ProjectTemplateType, throwIfNotFound: true);
			var xElement = new XElement("Project");
			XmlUtils.SetAttributeValue(xElement, "Guid", databaseObject.Guid);
			XmlUtils.SetAttributeValue(xElement, "Name", "GameProject");
			XmlUtils.SetAttributeValue(xElement, "Version", VersionsManager.SerializationVersion);
            XmlUtils.SetAttributeValue(xElement, "APIVersion", ModsManager.ApiVersionString);
            var xElement2 = new XElement("Subsystems");
			xElement.Add(xElement2);
            var xElement3 = new XElement("Values");
			XmlUtils.SetAttributeValue(xElement3, "Name", "GameInfo");
			valuesDictionary.Save(xElement3);
			xElement2.Add(xElement3);
			var xElement4 = new XElement("Values");
			XmlUtils.SetAttributeValue(xElement4, "Name", "Players");
			valuesDictionary2.Save(xElement4);
			xElement2.Add(xElement4);
			using (Stream stream = Storage.OpenFile(Storage.CombinePaths(unusedWorldDirectoryName, "Project.xml"), OpenFileMode.Create))
			{
				XmlUtils.SaveXmlToStream(xElement, stream, null, throwOnError: true);
			}
			return GetWorldInfo(unusedWorldDirectoryName);
		}

		public static void ChangeWorld(string directoryName, WorldSettings worldSettings)
		{
			string path = Storage.CombinePaths(directoryName, "Project.xml");
			if (!Storage.FileExists(path))
			{
				return;
			}
			XElement xElement = null;
			using (Stream stream = Storage.OpenFile(path, OpenFileMode.Read))
			{
				xElement = XmlUtils.LoadXmlFromStream(stream, null, throwOnError: true);
			}
			XElement gameInfoNode = GetGameInfoNode(xElement);
			var valuesDictionary = new ValuesDictionary();
			valuesDictionary.ApplyOverrides(gameInfoNode);
			GameMode value = valuesDictionary.GetValue<GameMode>("GameMode");
			worldSettings.Save(valuesDictionary, liveModifiableParametersOnly: true);
			gameInfoNode.RemoveNodes();
			valuesDictionary.Save(gameInfoNode);
			using (Stream stream2 = Storage.OpenFile(path, OpenFileMode.Create))
			{
				XmlUtils.SaveXmlToStream(xElement, stream2, null, throwOnError: true);
			}
			if (worldSettings.GameMode != value)
			{
				if (worldSettings.GameMode == GameMode.Adventure)
				{
					TakeWorldSnapshot(directoryName, "AdventureRestart");
				}
				else
				{
					DeleteWorldSnapshot(directoryName, "AdventureRestart");
				}
			}
		}

		public static string GetUnusedWorldDirectoryName()
		{
			string text = Storage.CombinePaths(WorldsDirectoryName, "World");
			for (int i = 0; i < 1000; i++)
			{
				string arg = Storage.CombinePaths(Storage.GetDirectoryName(text), Storage.GetFileNameWithoutExtension(text));
				string extension = Storage.GetExtension(text);
				string text2 = $"{arg}{((i > 0) ? i.ToString() : string.Empty)}{extension}";
				if (!Storage.DirectoryExists(text2) && !Storage.FileExists(text2))
				{
					return text2;
				}
			}
			throw new InvalidOperationException($"Out of filenames for root \"{text}\".");
		}

		public static void RecursiveEnumerateDirectory(string directoryName, List<string> files, List<string> directories, Func<string, bool> filesFilter)
		{
			try
			{
				foreach (string item in Storage.ListDirectoryNames(directoryName))
				{
					string text = Storage.CombinePaths(directoryName, item);
					RecursiveEnumerateDirectory(text, files, directories, filesFilter);
					directories?.Add(text);
				}
				if (files != null)
				{
					foreach (string item2 in Storage.ListFileNames(directoryName))
					{
						string text2 = Storage.CombinePaths(directoryName, item2);
						if (filesFilter == null || filesFilter(text2))
						{
							files.Add(text2);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error($"Error enumerating files/directories. Reason: {ex.Message}");
			}
		}

		public static XElement GetGameInfoNode(XElement projectNode)
		{
			XElement xElement = (from n in projectNode.Element("Subsystems").Elements("Values")
								 where XmlUtils.GetAttributeValue(n, "Name", string.Empty) == "GameInfo"
								 select n).FirstOrDefault();
			if (xElement != null)
			{
				return xElement;
			}
			throw new InvalidOperationException("GameInfo node not found in project.");
		}

        public static XElement GetSubsystemNode(XElement projectNode, string subsystemName)
        {
            XElement xElement = (from n in projectNode.Element("Subsystems").Elements("Values")
                                 where XmlUtils.GetAttributeValue(n, "Name", string.Empty) == subsystemName
                                 select n).FirstOrDefault();
            if (xElement != null)
            {
                return xElement;
            }
            throw new InvalidOperationException(subsystemName + " node not found in project.");
        }

        public static XElement GetPlayersNode(XElement projectNode)
		{
			XElement xElement = (from n in projectNode.Element("Subsystems").Elements("Values")
								 where XmlUtils.GetAttributeValue(n, "Name", string.Empty) == "Players"
								 select n).FirstOrDefault();
			if (xElement != null)
			{
				return xElement;
			}
			throw new InvalidOperationException("Players node not found in project.");
		}

		public static void PackWorld(string directoryName, Stream targetStream, Func<string, bool> filter, bool embedExternalContent)
		{
			WorldInfo worldInfo = GetWorldInfo(directoryName);
			if (worldInfo == null)
			{
				throw new InvalidOperationException("Directory does not contain a world.");
			}
			var list = new List<string>();
			RecursiveEnumerateDirectory(directoryName, list, null, filter);
			using var zipArchive = ZipArchive.Create(targetStream,keepStreamOpen: true);
			foreach(string item in list)
			{
				using Stream source = Storage.OpenFile(item,OpenFileMode.Read);
				string fileName = Storage.GetFileName(item);
				fileName = fileName.StartsWith("Region") ? Storage.CombinePaths("Regions",fileName) : item.Replace($"{directoryName}/","");
				zipArchive.AddStream(fileName,source);
			}
			if(embedExternalContent)
			{
				if(!BlocksTexturesManager.IsBuiltIn(worldInfo.WorldSettings.BlocksTextureName))
				{
					try
					{
						using Stream source2 = Storage.OpenFile(BlocksTexturesManager.GetFileName(worldInfo.WorldSettings.BlocksTextureName),OpenFileMode.Read);
						string filenameInZip = Storage.CombinePaths("EmbeddedContent",Storage.GetFileNameWithoutExtension(worldInfo.WorldSettings.BlocksTextureName) + ".scbtex");
						zipArchive.AddStream(filenameInZip,source2);
					}
					catch(Exception ex)
					{
						Log.Warning($"Failed to embed blocks texture \"{worldInfo.WorldSettings.BlocksTextureName}\". Reason: {ex.Message}");
					}
				}
				foreach(PlayerInfo playerInfo in worldInfo.PlayerInfos)
				{
					if(!CharacterSkinsManager.IsBuiltIn(playerInfo.CharacterSkinName))
					{
						try
						{
							using Stream source3 = Storage.OpenFile(CharacterSkinsManager.GetFileName(playerInfo.CharacterSkinName),OpenFileMode.Read);
							string filenameInZip2 = Storage.CombinePaths("EmbeddedContent",Storage.GetFileNameWithoutExtension(playerInfo.CharacterSkinName) + ".scskin");
							zipArchive.AddStream(filenameInZip2,source3);
						}
						catch(Exception ex2)
						{
							Log.Warning($"Failed to embed character skin \"{playerInfo.CharacterSkinName}\". Reason: {ex2.Message}");
						}
					}
				}
			}
		}

		public static void UnpackWorld(string directoryName, Stream sourceStream, bool importEmbeddedExternalContent)
		{
			if (!Storage.DirectoryExists(directoryName))
			{
				throw new InvalidOperationException($"Cannot import world into \"{directoryName}\" because this directory does not exist.");
			}
			using var zipArchive = ZipArchive.Open(sourceStream,keepStreamOpen: true);
			foreach(ZipArchiveEntry item in zipArchive.ReadCentralDir())
			{
				string text = item.FilenameInZip.Replace('\\','/');
				string extension = Storage.GetExtension(text);
				if(text.StartsWith("EmbeddedContent"))
				{
					try
					{
						if(importEmbeddedExternalContent)
						{
							var memoryStream = new MemoryStream();
							zipArchive.ExtractFile(item,memoryStream);
							memoryStream.Position = 0L;
							ExternalContentType type = ExternalContentManager.ExtensionToType(extension);
							ExternalContentManager.ImportExternalContentSync(memoryStream,type,Storage.GetFileNameWithoutExtension(text));
						}
					}
					catch(Exception ex)
					{
						Log.Warning($"Failed to import embedded content \"{text}\". Reason: {ex.Message}");
					}
				}
				else
				{
					string fileName = Storage.GetFileName(text);
					if(fileName.StartsWith("Region"))
					{
						if(!Storage.DirectoryExists(Storage.CombinePaths(directoryName,"Regions")))
						{
							Storage.CreateDirectory(Storage.CombinePaths(directoryName,"Regions"));
						}
						fileName = Storage.CombinePaths("Regions",fileName);
					}
					else
					{
						string directory = Path.GetDirectoryName(Storage.CombinePaths(directoryName,text));
						if(!Storage.DirectoryExists(directory))
						{
							Storage.CreateDirectory(directory);
						}
						fileName = text;
					}
					using Stream stream = Storage.OpenFile(Storage.CombinePaths(directoryName,fileName),OpenFileMode.Create);
					zipArchive.ExtractFile(item,stream);
				}
			}
		}

		public static void DeleteWorldContents(string directoryName, Func<string, bool> filter)
		{
			var list = new List<string>();
			var list2 = new List<string>();
			RecursiveEnumerateDirectory(directoryName, list, list2, filter);
			foreach (string item in list)
			{
				Storage.DeleteFile(item);
			}
			foreach (string item2 in list2)
			{
				Storage.DeleteDirectory(item2);
			}
		}

		public static string MakeSnapshotFilename(string directoryName, string snapshotName)
		{
			return Storage.CombinePaths(directoryName, $"{snapshotName}.snapshot");
		}

		public static bool TestXmlFile(string fileName, string rootNodeName)
		{
			try
			{
				if (Storage.FileExists(fileName))
				{
					using Stream stream = Storage.OpenFile(fileName,OpenFileMode.Read);
					XElement xElement = XmlUtils.LoadXmlFromStream(stream,null,throwOnError: false);
					return xElement != null && xElement.Name == rootNodeName;
				}
				return false;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
