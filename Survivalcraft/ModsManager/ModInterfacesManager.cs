using System;
using System.Collections.Generic;
using System.Linq;
using Engine;

namespace Game;

public static class ModInterfacesManager
{
    private static List<ModInterface> m_interfaces = [];
    public static ReadOnlyList<ModInterface> Interfaces => new(m_interfaces);
    public static void RegisterInterface<TModInterface>(IModLoader loader) where TModInterface : ModInterface
    {
        var type = typeof(TModInterface);
        if (Activator.CreateInstance(type, [loader]) is not TModInterface instance)
        {
            throw new InvalidOperationException($"无法实例化类型 {type.FullName}。");
        }
        m_interfaces.Add(instance);
        instance._InterfaceInitialized();
    }
    public static void InvokeHooks<TModInterface>(string hookName, HookInvoker<TModInterface> invoker,
        ModEntity? modEntity = null) where TModInterface : ModInterface
    {
        if (modEntity is not null)
        {
            InvokeHooksOfMod(hookName, invoker, modEntity);
            return;
        }

        foreach (TModInterface modInterface in ModInterfacesManager.Interfaces
                     .Where(modInterface => modInterface.RegisteredHooks.Contains(hookName))
                     .OfType<TModInterface>())
        {
            invoker(modInterface, out bool isContinueRequired);
            if (!isContinueRequired) break;
        }
    }

    private static void InvokeHooksOfMod<TModInterface>(string hookName, HookInvoker<TModInterface> invoker,
        ModEntity modEntity) where TModInterface : ModInterface
    {
        foreach (TModInterface modInterface in ModInterfacesManager.Interfaces
                     .Where(modInterface => modInterface.RegisteredHooks.Contains(hookName) &&
                                            Equals(modInterface.ModEntity, modEntity))
                     .OfType<TModInterface>())
        {
            invoker(modInterface, out bool isContinueRequired);
            if (!isContinueRequired) break;
        }
    }

    public static TInterface? FindInterface<TInterface>() where TInterface : ModInterface
    {
        foreach (var modInterface in Interfaces)
        {
            if (modInterface is TInterface result)
            {
                return result;
            }
        }

        return null;
    }
}