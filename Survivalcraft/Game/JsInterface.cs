using System;
using System.Collections.Generic;
using System.Reflection;
using Engine;
using Engine.Input;
using Esprima.Ast;
using Game;
using GameEntitySystem;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using JsEngine = Jint.Engine;

namespace Game
{
    public class JsInterface
    {
        public static JsEngine engine;
        public static SurvivalCraftModLoader loader;
        public static Dictionary<string,List<FunctionInstance>> handlersDictionary;
        private static Project Project
        {
            get
            {
                return GameManager.Project;
            }
        }
        public static Project getProject()
        {
            return JsInterface.Project;
        }
        public static Subsystem findSubsystem(string name)
        {
            if (JsInterface.Project == null)
            {
                return null;
            }
            Type t = Type.GetType("Game.Subsystem" + name + ",Survivalcraft");
            if (t == null)
            {
                return null;
            }
            return JsInterface.Project.FindSubsystem(t, null, false);
        }
        public static void Initiate()
        {
            engine = new JsEngine(delegate (Jint.Options cfg)
            {
                cfg.AllowClr();
                cfg.AllowClr(new Assembly[]
                {
                    IntrospectionExtensions.GetTypeInfo(typeof(Program)).Assembly
                });
                cfg.AllowClr(new Assembly[]
                {
                    IntrospectionExtensions.GetTypeInfo(typeof(Matrix)).Assembly
                });
                cfg.AllowClr(new Assembly[]
                {
                    IntrospectionExtensions.GetTypeInfo(typeof(Program)).Assembly
                });
                cfg.AllowClr(new Assembly[]
                {
                    IntrospectionExtensions.GetTypeInfo(typeof(Project)).Assembly
                });
                cfg.AllowClr(new Assembly[]
                {
                    IntrospectionExtensions.GetTypeInfo(typeof(JsInterface)).Assembly
                });
                cfg.DebugMode(true);
            });
            string initCode = Storage.ReadAllText("app:init.js");
            Execute(initCode);
        }
        public static void RegisterEvent()
        {
            FunctionInstance keyDown = engine.GetValue("keyDown").AsFunctionInstance();
            Keyboard.KeyDown += delegate (Key key)
            {
                Invoke(keyDown, key.ToString());
            };
            FunctionInstance keyUp = engine.GetValue("keyUp").AsFunctionInstance();
            Keyboard.KeyUp += delegate (Key key)
            {
                Invoke(keyUp, key.ToString());
            };
            List<FunctionInstance> array = GetHandlers("frameHandlers");
            if (array != null && array.Count > 0)
            {
                Window.Frame += delegate ()
                {
                    array.ForEach(function =>
                    {
                        Invoke(function);
                    });
                };
            }
            handlersDictionary = new Dictionary<string, List<FunctionInstance>>();
            List<ModLoader> mods = ModsManager.ModLoaders;
            loader = (SurvivalCraftModLoader)ModsManager.ModLoaders.Find((item) => item is SurvivalCraftModLoader);
            GetAndRegisterHandlers("OnMinerDig");
            GetAndRegisterHandlers("OnMinerPlace");
            GetAndRegisterHandlers("OnPlayerSpawned");
            GetAndRegisterHandlers("OnPlayerDead");
            GetAndRegisterHandlers("AttackBody");
            GetAndRegisterHandlers("OnCreatureInjure");
            GetAndRegisterHandlers("OnProjectLoaded");
            GetAndRegisterHandlers("OnProjectDisposed");
        }
        public static void Execute(string str)
        {
            try
            {
                engine.Execute(str);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
        public static void Execute(Script script)
        {
            try
            {
                engine.Execute(script);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
        public static JsValue Invoke(string str,params object[] arguments)
        {
            try
            {
                return engine.Invoke(str,arguments);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            return null;
        }
        public static JsValue Invoke(JsValue jsValue, params object[] arguments)
        {
            try
            {
                return engine.Invoke(jsValue, arguments);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            return null;
        }
        public static List<FunctionInstance> GetHandlers(string str)
        {
            JsArray array = engine.GetValue(str).AsArray();
            if (array.IsNull()) return null;
            List<FunctionInstance> list = new List<FunctionInstance>();
            foreach (JsValue item in array)
            {
                try
                {
                    FunctionInstance function = item.AsFunctionInstance();
                    if (!function.IsNull())
                    {
                        list.Add(function);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
            return list;
        }
        public static void GetAndRegisterHandlers(string handlesName)
        {
            try
            {
                if (handlersDictionary.ContainsKey(handlesName)) return;
                List<FunctionInstance> handlers = GetHandlers($"{handlesName}Handlers");
                if (handlers != null && handlers.Count > 0)
                {
                    handlersDictionary.Add(handlesName, handlers);
                    ModsManager.RegisterHook(handlesName, loader);
                }
            }catch(Exception ex) { 
                Console.WriteLine(ex);
            }
        }
    }
}