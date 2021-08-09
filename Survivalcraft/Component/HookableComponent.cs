using Engine;
using GameEntitySystem;
using System;
using TemplatesDatabase;
using System.Collections.Generic;

namespace Game
{
    public class HookableComponent<T> : Component where T:class
    {
        private Dictionary<string, ModsManager.ModHook> Hooks = new Dictionary<string, ModsManager.ModHook>();

        public override void Dispose()
        {
            foreach (var item in Hooks)
            {
                Hooks[item.Key] = null;
            }
        }

        public void HookActions() {
            foreach (var item in Hooks) {
                item.Value.HookAction();
            }        
        }

        public void Hook(ModLoader modLoader,string name, Action<T> action)
        {
            if (modLoader == null) return;
            if (Hooks.TryGetValue(name, out ModsManager.ModHook modHook))
            {
                modHook.AddHook(modLoader, action);
            }
            else
            { modHook = new ModsManager.ModHook(name);
                modHook.AddHook(modLoader,action);
                Hooks.Add(name, modHook);
            }
        }

    }
}
