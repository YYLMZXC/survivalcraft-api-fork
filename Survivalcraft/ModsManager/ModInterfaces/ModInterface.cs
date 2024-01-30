using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Engine;

namespace Game;

public abstract class ModInterface
{
    public IModLoader ParentModLoader { get; internal set; }
    public ModEntity ModEntity => ParentModLoader.ModEntity;

    protected internal ModInterface(IModLoader parent)
    {
        ParentModLoader = parent;
    }

    protected internal virtual void _InterfaceInitialized()
    {
    }

    public abstract string[] AvailableHooks { get; }
    
    internal void InternalRegisterHook(string hookName, [CallerMemberName] string caller = null!,
        [CallerLineNumber] int line = default)
    {
        if (!AvailableHooks.Contains(hookName))
        {
            throw new InvalidOperationException($"Hook {hookName} not found.");
        }

        if (RegisteredHooks.Contains(hookName))
        {
            Log.Warning($"重复注册 hook : {hookName}, at {caller}, line {line}");
            return;
        }

        RegisteredHooks.Add(hookName);
    }

    protected internal List<string> RegisteredHooks { get; } = [];

    protected void RegisterHook(string hookName)
    {
        InternalRegisterHook(hookName);
    }
}