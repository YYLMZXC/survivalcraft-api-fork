using Engine;
using GameEntitySystem;
using System;
using TemplatesDatabase;
using System.Collections.Generic;

namespace Game
{
    public class HookableSubsystem<T> : Subsystem where T : class
    {
        private class SubsystemHook
        {
            public Dictionary<ModLoader, bool> Loaders = new Dictionary<ModLoader, bool>();//是否被禁用
            public Dictionary<ModLoader, Action<T>> Hooks = new Dictionary<ModLoader, Action<T>>();
            public Dictionary<ModLoader, string> DisableReason = new Dictionary<ModLoader, string>();
            public string HookName;
            public SubsystemHook(string name)
            {
                HookName = name;
            }
            public void AddHook(ModLoader modLoader, Action<T> action)
            {
                if (action != null)
                {

                    if (Hooks.TryGetValue(modLoader, out Action<T> action1) == false)
                    {
                        Hooks.Add(modLoader, action1);
                    }
                    else
                    {
                        action1 += action;
                    }
                }
            }
            public void Disable(ModLoader from, ModLoader toDisable, string reason)
            {
                if (Loaders.TryGetValue(toDisable, out bool k))
                {
                    k = false;
                    if (DisableReason.TryGetValue(from, out string res))
                    {
                        res = reason;
                    }
                    else
                    {
                        DisableReason.Add(from, reason);
                    }
                }
            }
            public void HookAction(T obj)
            {
                foreach (var item in Hooks)
                {
                    if (Loaders.TryGetValue(item.Key, out bool k) && k)
                    {
                        item.Value?.Invoke(obj);
                    }
                }
            }
        }
        private Dictionary<string, SubsystemHook> Hooks = new Dictionary<string, SubsystemHook>();
        public void HookActions(T obj, string name)
        {
            if (Hooks.TryGetValue(name, out SubsystemHook subsystem)) {
                subsystem.HookAction(obj);
            }
        }

        public void Hook(string HookName, ModLoader modLoader, Action<T> action)
        {
            if (modLoader == null) return;
            if (Hooks.TryGetValue(HookName, out SubsystemHook component))
            {
                component.AddHook(modLoader, action);
            }
            else
            {
                component = new SubsystemHook(HookName);
                component.AddHook(modLoader, action);
                Hooks.Add(HookName, component);
            }
        }
        public void Disable(string HookName, ModLoader from, ModLoader toDisable, string reason)
        {
            if (Hooks.TryGetValue(HookName, out SubsystemHook component))
            {
                component.Disable(from, toDisable, reason);
            }
        }

    }
}
