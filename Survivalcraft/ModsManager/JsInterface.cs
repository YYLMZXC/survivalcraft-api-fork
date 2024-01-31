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
	public static class JsInterface
	{
		public static JsEngine engine;
		private static InterfaceImplementForJs m_interfaceImplementForJs = null!;
		
		public static Dictionary<string, List<FunctionInstance>> handlersDictionary = [];
		private static Project? Project => GameManager.Project;

		public static Project? getProject() => Project;

		public static Subsystem? findSubsystem(string name)
		{
			if (Project is null)
			{
				return null;
			}
			Type? t = Type.GetType("Game.Subsystem" + name + ",Survivalcraft");
			return t == null ? null : Project.FindSubsystem(t, null, false);
		}
		public static void Initiate()
		{
			engine = new JsEngine( cfg =>
			{
				cfg.AllowClr([
					typeof(Program).Assembly,
					typeof(Matrix).Assembly,
					typeof(Project).Assembly
				]);
				cfg.DebugMode();
			});
			string initCode;
			if (VersionsManager.Platform == Platform.Android)
			{
				//TODO: 适配安卓端
				return;
			}
			else
			{
				initCode = Storage.ReadAllText(ModsManager.ExtPath + "init.js");
			}
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
			List<FunctionInstance>? instances = GetHandlers("frameHandlers");
			if (instances is { Count: > 0 })
			{
				Window.Frame += () => instances.ForEach(InvokeVoid);
			}

			m_interfaceImplementForJs = ModInterfacesManager.FindInterface<InterfaceImplementForJs>()!;
			
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
				Log.Error(ex);
				return ex.ToString();
			}
		}
		public static JsValue? Invoke(string str, params object[] arguments)
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
		public static JsValue? Invoke(this JsValue jsValue, params object[] arguments)
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
		private static void InvokeVoid(FunctionInstance func)
		{
			try
			{
				engine.Invoke(func, []);
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
		}
		public static List<FunctionInstance> GetHandlers(string str)
		{
			JsArray array = engine.GetValue(str).AsArray();
			try
			{
				List<FunctionInstance> list = array.IsNull() ? [] : [..array.Select(item => item.AsFunctionInstance())];
				return list;
			}
			catch (Exception ex)
			{
				Log.Error(ex);
				return [];
			}
		}
		public static void GetAndRegisterHandlers(string handlesName)
		{
			try
			{
				if (handlersDictionary.ContainsKey(handlesName)) return;
				List<FunctionInstance> handlers = GetHandlers($"{handlesName}Handlers");
				if (handlers.Count <= 0) return;
				
				handlersDictionary.Add(handlesName, handlers);
				m_interfaceImplementForJs.Register(handlesName);
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
		}
	}
}