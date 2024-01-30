using Engine;
using Engine.Input;
using Esprima.Ast;
using GameEntitySystem;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsEngine = Jint.Engine;

namespace Game
{
	public class JsInterface
	{
		public static JsEngine engine;
		public static JsModLoader? JsModLoader;
		public static Dictionary<string, List<FunctionInstance>> handlersDictionary;
		private static Project? Project => GameManager.Project;

		public static Project? getProject() => Project;

		public static Subsystem? findSubsystem(string name)
		{
			if (Project is null)
			{
				return null;
			}
			Type t = Type.GetType("Game.Subsystem" + name + ",Survivalcraft");
			return t == null ? null : Project.FindSubsystem(t, null, false);
		}
		public static void Initiate()
		{
			//TODO: 把 SurvivalcraftModLoader 中有关 JavaScript 的内容移动到 JsModLoader 中
			return;
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
			return;
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
			//TODO: 在移动完 SurvivalCraftModLoader 中的内容后取消 return
			handlersDictionary = [];
			JsModLoader = ModsManager.ModLoaders.FirstOrDefault(loader => loader is JsModLoader) as JsModLoader;
			//把 linq 查询改为代码，合并两次类型检查
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
			return;
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
		public static JsValue Invoke(string str, params object[] arguments)
		{
			try
			{
				return engine.Invoke(str, arguments);
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
			List<FunctionInstance> list = [];
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
					//ModsManager.RegisterHook(handlesName, SurvivalCraftModInterface);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
		}
	}
}