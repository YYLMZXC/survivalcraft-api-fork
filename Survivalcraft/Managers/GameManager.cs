using Engine;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using TemplatesDatabase;
using XmlUtilities;

namespace Game
{
	public static class GameManager
	{
		public static WorldInfo m_worldInfo;

		public static Project m_project;

		public static SubsystemUpdate m_subsystemUpdate;

		public static ManualResetEvent m_saveCompleted = new(initialState: true);

		public static Project Project => m_project;

		public static WorldInfo WorldInfo => m_worldInfo;

		public static List<Func<bool>> SyncDispatcher = [];

		public static event Action<Project> ProjectDisposed;

		public static void LoadProject(WorldInfo worldInfo, ContainerWidget gamesWidget)
		{
			DisposeProject();
			WorldsManager.RepairWorldIfNeeded(worldInfo.DirectoryName);
			VersionsManager.UpgradeWorld(worldInfo.DirectoryName);
            BlocksManager.LoadBlocksStaticly = string.IsNullOrEmpty(worldInfo.APIVersion);
            using (Stream stream = Storage.OpenFile(Storage.CombinePaths(worldInfo.DirectoryName, "Project.xml"), OpenFileMode.Read))
			{
				var valuesDictionary = new ValuesDictionary();
				var valuesDictionary2 = new ValuesDictionary();
				valuesDictionary.SetValue("GameInfo", valuesDictionary2);
				valuesDictionary2.SetValue("WorldDirectoryName", worldInfo.DirectoryName);
				var valuesDictionary3 = new ValuesDictionary();
				valuesDictionary.SetValue("Views", valuesDictionary3);
				valuesDictionary3.SetValue("GamesWidget", gamesWidget);
				XElement projectNode = XmlUtils.LoadXmlFromStream(stream, null, throwOnError: true);
				ModsManager.HookAction("ProjectXmlLoad", loader =>
				{
					loader.ProjectXmlLoad(projectNode);
					return false;
				});
				Project.OnProjectLoad += delegate (Project project)
				{
					ModsManager.HookAction("OnProjectLoaded", loader => { loader.OnProjectLoaded(project); return false; });
				};
				Project.EntityAdded += new EventHandler<EntityAddRemoveEventArgs>((s, arg) =>
				{
					ModsManager.HookAction("OnEntityAdd", loader =>
					{
						loader.OnEntityAdd(arg.Entity);
						return false;
					});
				});
				Project.EntityRemoved += new EventHandler<EntityAddRemoveEventArgs>((s, arg) =>
				{
					ModsManager.HookAction("OnEntityRemove", loader =>
					{
						loader.OnEntityRemove(arg.Entity);
						return false;
					});
				});
				var projectData = new ProjectData(DatabaseManager.GameDatabase, projectNode, valuesDictionary, ignoreInvalidEntities: true);
				m_project = new Project(DatabaseManager.GameDatabase, projectData);
				m_subsystemUpdate = m_project.FindSubsystem<SubsystemUpdate>(throwOnError: true);
			}
			m_worldInfo = worldInfo;
			Log.Information("Loaded world, GameMode={0}, StartingPosition={1}, WorldName={2}, VisibilityRange={3}, Resolution={4}", worldInfo.WorldSettings.GameMode, worldInfo.WorldSettings.StartingPositionMode, worldInfo.WorldSettings.Name, SettingsManager.VisibilityRange.ToString(), SettingsManager.ResolutionMode.ToString());
			GC.Collect();
		}

		public static void SaveProject(bool waitForCompletion, bool showErrorDialog)
		{
			if (m_project != null)
			{
				double realTime = Time.RealTime;
				ProjectData projectData = m_project.Save();
				m_saveCompleted.WaitOne();
				m_saveCompleted.Reset();
				SubsystemGameInfo subsystemGameInfo = m_project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
				string projectFileName = Storage.CombinePaths(subsystemGameInfo.DirectoryName, "Project.xml");
				Exception e = default;
				Task.Run(delegate
				{
					try
					{
						WorldsManager.MakeQuickWorldBackup(subsystemGameInfo.DirectoryName);
						var xElement = new XElement("Project");
						ModsManager.HookAction("ProjectXmlSave", loader =>
						{
							loader.ProjectXmlSave(xElement);
							return false;
						});
						projectData.Save(xElement);
						XmlUtils.SetAttributeValue(xElement, "Version", VersionsManager.SerializationVersion);
                        XmlUtils.SetAttributeValue(xElement, "APIVersion", ModsManager.ApiVersionString);
                        Storage.CreateDirectory(subsystemGameInfo.DirectoryName);
						using (Stream stream = Storage.OpenFile(projectFileName, OpenFileMode.Create))
						{
							XmlUtils.SaveXmlToStream(xElement, stream, null, throwOnError: true);
						}
					}
					catch (Exception ex)
					{
						e = ex;
						if (showErrorDialog)
						{
							Dispatcher.Dispatch(delegate
							{
								DialogsManager.ShowDialog(null, new MessageDialog("Error saving game", e.Message, "OK", null, null));
							});
						}
					}
					finally
					{
						m_saveCompleted.Set();
					}
				});
				if (waitForCompletion)
				{
					m_saveCompleted.WaitOne();
				}
				double realTime2 = Time.RealTime;
				Log.Verbose($"Saved project, {Math.Round((realTime2 - realTime) * 1000.0)}ms");
			}
		}

		public static void UpdateProject()
		{
			if (SyncDispatcher.Count > 0 && SyncDispatcher[0].Invoke())
			{
				SyncDispatcher.RemoveAt(0);
			}
			if (m_project != null)
			{
				m_subsystemUpdate.Update();
			}
		}

		public static void DisposeProject()
		{
			if (m_project != null)
			{
				ProjectDisposed?.Invoke(m_project);
				m_project.Dispose();
				m_project = null;
				m_subsystemUpdate = null;
				m_worldInfo = null;
				ModsManager.HookAction("OnProjectDisposed", loader => { loader.OnProjectDisposed(); return false; });
				GC.Collect();
			}
		}
	}
}
