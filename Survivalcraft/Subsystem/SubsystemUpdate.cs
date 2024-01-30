using Engine;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemUpdate : Subsystem
	{
		public class UpdateableInfo
		{
			public UpdateOrder UpdateOrder;
		}

		public class Comparer : IComparer<IUpdateable>
		{
			public static Comparer Instance = new();

			public int Compare(IUpdateable u1, IUpdateable u2)
			{
				int num = u1.UpdateOrder - u2.UpdateOrder;
				if (num != 0)
				{
					return num;
				}
				return u1.GetHashCode() - u2.GetHashCode();
			}
		}

		public SubsystemTime m_subsystemTime;

		public Dictionary<IUpdateable, UpdateableInfo> m_updateables = [];

		public Dictionary<IUpdateable, bool> m_toAddOrRemove = [];

		public List<IUpdateable> m_sortedUpdateables = [];

		public int UpdateablesCount => m_updateables.Count;

		public int UpdatesPerFrame
		{
			get;
			set;
		}

		public void Update()
		{
			for (int i = 0; i < UpdatesPerFrame; i++)
			{
				m_subsystemTime.NextFrame();
				bool flag = false;
				foreach (KeyValuePair<IUpdateable, bool> item in m_toAddOrRemove)
				{
					if (item.Value)
					{
						m_updateables.Add(item.Key, new UpdateableInfo
						{
							UpdateOrder = item.Key.UpdateOrder
						});
						flag = true;
					}
					else
					{
						m_updateables.Remove(item.Key);
						flag = true;
					}
				}
				m_toAddOrRemove.Clear();
				foreach (KeyValuePair<IUpdateable, UpdateableInfo> updateable in m_updateables)
				{
					UpdateOrder updateOrder = updateable.Key.UpdateOrder;
					if (updateOrder != updateable.Value.UpdateOrder)
					{
						flag = true;
						updateable.Value.UpdateOrder = updateOrder;
					}
				}
				if (flag)
				{
					m_sortedUpdateables.Clear();
					foreach (IUpdateable key in m_updateables.Keys)
					{
						m_sortedUpdateables.Add(key);
					}
					m_sortedUpdateables.Sort(Comparer.Instance);
				}
				float dt = MathUtils.Clamp(m_subsystemTime.GameTimeDelta, 0f, 0.1f);
				foreach (IUpdateable sortedUpdateable in m_sortedUpdateables)
				{
					try
					{
						sortedUpdateable.Update(dt);
					}
					catch (Exception)
					{
					}
				}

				ModInterfacesManager.InvokeHooks("SubsystemUpdate",
					(SurvivalCraftModInterface modInterface, out bool isContinueRequired) =>
					{
						modInterface.SubsystemUpdate(dt);
						isContinueRequired = true;
					});
			}
		}

		public void AddUpdateable(IUpdateable updateable)
		{
			m_toAddOrRemove[updateable] = true;
		}

		public void RemoveUpdateable(IUpdateable updateable)
		{
			m_toAddOrRemove[updateable] = false;
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
			foreach (IUpdateable item in Project.FindSubsystems<IUpdateable>())
			{
				AddUpdateable(item);
			}
			UpdatesPerFrame = 1;
		}

		public override void OnEntityAdded(Entity entity)
		{
			foreach (IUpdateable item in entity.FindComponents<IUpdateable>())
			{
				AddUpdateable(item);
			}
		}

		public override void OnEntityRemoved(Entity entity)
		{
			foreach (IUpdateable item in entity.FindComponents<IUpdateable>())
			{
				RemoveUpdateable(item);
			}
		}
	}
}
