using Acornima.Ast;
using Engine;
using Engine.Input;
using GameEntitySystem;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using System.Reflection;
using System.Text;
using JsEngine = Jint.Engine;

namespace Game
{
	public class JsInterface
	{
		public static JsEngine engine;
		public static SurvivalCraftModLoader loader;
		public static Dictionary<string, List<Function>> handlersDictionary;

		public static bool CheckInitJsExists()
		{
			if (ModsManager.IsAndroid)
			{
				return true;
			}
			string fullPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location == "" ? AppContext.BaseDirectory : Assembly.GetExecutingAssembly().Location);//路径备选方案
			string path = Path.Combine(fullPath, "init.js");
			if (!File.Exists(path))
			{
				using (FileStream destination = new FileStream(path, FileMode.Create))
				{
					Assembly.GetExecutingAssembly().GetManifestResourceStream("Game.init.js").CopyTo(destination);
				}
			}
			return File.Exists(path);
		}
		public static void Initiate()
		{
			engine = new JsEngine(delegate (Options cfg)
			{
				cfg.AllowClr();
				cfg.AllowClr(
					typeof(Program).GetTypeInfo().Assembly//Game Namespace
				);
				cfg.AllowClr(
					typeof(Matrix).GetTypeInfo().Assembly//Engine Namespace
				);
				cfg.AllowClr(
					typeof(Project).GetTypeInfo().Assembly//GameEntitySystem Namespace
				);
				cfg.DebugMode();
			});
			string codeString = null;
			if (CheckInitJsExists())
			{
				
				try
				{
					if (ModsManager.IsAndroid)
					{
						var file = ModsManager.StreamToBytes(
							typeof(JsInterface).Assembly.GetManifestResourceStream("Game.init.js"));
						codeString = Encoding.UTF8.GetString(file);
					}
					else
					{
						codeString = Storage.ReadAllText("app:init.js");
					}
				}
				catch
				{
					Log.Warning("Init.js未加载");
				}
				Execute(codeString);
			}
			else
			{
				Log.Warning("Init.js不存在");
			}
		}
		public static void RegisterEvent()
		{
			List<Function> keyDownHandlers = GetHandlers("keyDownHandlers");
			if (keyDownHandlers != null && keyDownHandlers.Count > 0)
			{
				Keyboard.KeyDown += delegate (Key key)
				{
					string keyString = key.ToString();
					keyDownHandlers.ForEach(function =>
					{
						Invoke(function, keyString);
					});
				};
			}
			List<Function> keyUpHandlers = GetHandlers("keyUpHandlers");
			if (keyUpHandlers != null && keyUpHandlers.Count > 0)
			{
				Keyboard.KeyUp += delegate (Key key)
				{
					string keyString = key.ToString();
					keyUpHandlers.ForEach(function =>
					{
						Invoke(function, keyString);
					});
				};
			}
			List<Function> frameHandlers = GetHandlers("frameHandlers");
			if (frameHandlers != null && frameHandlers.Count > 0)
			{
				Window.Frame += delegate ()
				{
					frameHandlers.ForEach(function =>
					{
						Invoke(function);
					});
				};
			}
			handlersDictionary = [];
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
		public static void Execute(in Prepared<Script> script)
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
		public static List<Function> GetHandlers(string str)
		{
			JsArray array = engine.GetValue(str).AsArray();
			if (array.IsNull()) return null;
			List<Function> list = [];
			foreach (JsValue item in array)
			{
				try
				{
					Function function = item.AsFunctionInstance();
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
				List<Function> handlers = GetHandlers($"{handlesName}Handlers");
				if (handlers != null && handlers.Count > 0)
				{
					handlersDictionary.Add(handlesName, handlers);
					ModsManager.RegisterHook(handlesName, loader);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
	}
}