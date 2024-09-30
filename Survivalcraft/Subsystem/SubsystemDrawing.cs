using GameEntitySystem;
using System;
using System.Collections.Generic;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemDrawing : Subsystem
	{
		public Dictionary<IDrawable, bool> m_drawables = [];

		public SortedMultiCollection<int, IDrawable> m_sortedDrawables = [];

		public int DrawablesCount => m_drawables.Count;

		public void AddDrawable(IDrawable drawable)
		{
			lock (m_drawables)
			{
                m_drawables.Add(drawable, value: true);
            }
		}

		public void RemoveDrawable(IDrawable drawable)
		{
			lock (m_drawables)
            {
                m_drawables.Remove(drawable);
            }
		}

		public void Draw(Camera camera)
		{
			m_sortedDrawables.Clear();
			lock (m_drawables)
			{
                foreach (IDrawable key2 in m_drawables.Keys)
                {
                    int[] drawOrders = key2.DrawOrders;
                    foreach (int key in drawOrders)
                    {
                        m_sortedDrawables.Add(key, key2);
                    }
                }
            }
			for (int j = 0; j < m_sortedDrawables.Count; j++)
			{
				try
				{
					KeyValuePair<int, IDrawable> keyValuePair = m_sortedDrawables[j];
					keyValuePair.Value.Draw(camera, keyValuePair.Key);
				}
				catch (Exception)
				{
				}
			}
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			foreach (IDrawable item in Project.FindSubsystems<IDrawable>())
			{
				AddDrawable(item);
			}
		}

        public override void OnEntityAdded(Entity entity)
        {
            foreach (IDrawable item in entity.FindComponents<IDrawable>())
            {
                bool skipVanilla = false;
                ModsManager.HookAction("OnIDrawableAdded", loader =>
                {
                    loader.OnIDrawableAdded(this, item, skipVanilla, out bool skip);
                    skipVanilla |= skip;
                    return false;
                });
                if (!skipVanilla)
                {
                    AddDrawable(item);
                }
            }
        }

        public override void OnEntityRemoved(Entity entity)
		{
			foreach (IDrawable item in entity.FindComponents<IDrawable>())
			{
				RemoveDrawable(item);
			}
		}
	}
}
