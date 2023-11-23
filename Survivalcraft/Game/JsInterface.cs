using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Engine;
using Engine.Input;
using Esprima.Ast;
using GameEntitySystem;
using Jint;
using Jint.Native;
using Jint.Native.Array;
using JsEngine = Jint.Engine;

namespace Game
{
    public class JsInterface
    {
        public static JsEngine engine;
        public static SurvivalCraftModLoader loader;
        public static Dictionary<string,List<JsValue>> handlersDictionary;
        private static Project Project
        {
            get
            {
                return GameManager.Project;
            }
        }
        public static Project getProject()
        {
            return Project;
        }
        public static Subsystem findSubsystem(string name)
        {
            if (Project == null)
            {
                return null;
            }
            Type t = Type.GetType("Game.Subsystem" + name + ",Survivalcraft");
            if (t == null)
            {
                return null;
            }
            return Project.FindSubsystem(t, null, false);
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
            var s = typeof(JsInterface).Assembly.GetManifestResourceStream("Game.init.js");
            string initCode = new StreamReader(s).ReadToEnd();
            Execute(initCode);
        }
        public static void RegisterEvent()
        {
            var keyDown = engine.GetValue("keyDown");
            Keyboard.KeyDown += delegate (Key key)
            {
                Invoke(keyDown, key.ToString());
            };
            var keyUp = engine.GetValue("keyUp");
            Keyboard.KeyUp += delegate (Key key)
            {
                Invoke(keyUp, key.ToString());
            };
            List<JsValue> array = GetHandlers("frameHandlers");
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

            handlersDictionary = new Dictionary<string, List<JsValue>>();
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
        public static string Evaluate(string str)
        {
            try
            {
                return engine.Evaluate(str).ToString();
            }
            catch (Exception ex)
            {
                string errors = ex.ToString();
                Log.Error(errors);
                return errors;
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
        public static List<JsValue> GetHandlers(string str)
        {
            ArrayInstance array = engine.GetValue(str).AsArray();
            List<JsValue> list = new List<JsValue>();
            for (int i = 0; i < array.Length; i++)
            {
                var obj = array.Get(i.ToString());
                list.Add(obj);
            }
            return list;
        }
        public static void GetAndRegisterHandlers(string handlesName)
        {
            try
            {
                if (handlersDictionary.ContainsKey(handlesName)) return;
                var handlers = GetHandlers($"{handlesName}Handlers");
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