﻿using System.Diagnostics;
using System.Net;
using Acornima.Ast;
using Engine;
using Engine.Input;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using System.Reflection;
using System.Text;
using System.Text.Json;
using JsEngine = Jint.Engine;
using System;
using System.IO;

namespace Game
{
	public class JsInterface
	{
		public static JsEngine engine;
		public static JsModLoader loader;
		public static Dictionary<string, List<Function>> handlersDictionary;

		public static HttpListener httpListener;
		public static int httpPort;
		public static string httpPassword;
		public static bool httpProcessing;
		public static bool httpScriptPrepared;
		public static Prepared<Script> httpScript;
		public static TaskCompletionSource<HttpResponse> httpResponse = new();

		public static bool CheckInitJsFileRelease()
		//改前:是安卓或者文件释放成功或外置 init.js存在返回真(不明确)
		//改后:本身有外置 init.js 或成功释放则返回真(最终有没有外置)
		{
			if (ModsManager.IsAndroid)
			{
				return false;
			}
			string fullPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location == "" ? AppContext.BaseDirectory : Assembly.GetExecutingAssembly().Location);//路径备选方案
			string path = Path.Combine(fullPath, "init.js");
/*
			if (!File.Exists(path))
			{
				using FileStream destination = new(path,FileMode.Create);
				Assembly.GetExecutingAssembly().GetManifestResourceStream("Game.init.js").CopyTo(destination);
			}
*/
			return File.Exists(path);
		}
		public static void Initiate()
			//修改前:先检查是否有外置 init.js 是安卓就直接加载内置,否则释放文件,释放成功就加载外置,否则报错
			//修改后:先检查是安卓就加载内置,否则释放并检查外部有 init.js 文件则加载外置否则加载内置(如果外置出现问题可以直接加载内置)
		{
			engine = new JsEngine(delegate (Options cfg)
			{
				cfg.AllowClr(AppDomain.CurrentDomain.GetAssemblies());
				cfg.DebugMode();
			});
			string codeString = null;
			try
			{
				if(CheckInitJsFileRelease())
				{
					codeString = Storage.ReadAllText("app:init.js");
				}
				else
				{
					using StreamReader reader = new(typeof(JsInterface).Assembly.GetManifestResourceStream("Game.init.js"),Encoding.UTF8);
					codeString = reader.ReadToEnd();
				}
			}
			catch
			{
				Log.Warning("Init.js加载失败");
			}

			Execute(codeString);
			httpListener = new HttpListener();
			if(ModsManager.Configs.TryGetValue("RemoteControlPort",out string portString) && int.TryParse(portString,out int port))
			{
				SetHttpPort(port);
			}
			else
			{
				SetHttpPort((DateTime.Now.Millisecond * 32749 + 8191) % 9000 + 1024,true);
			}
			if(ModsManager.Configs.TryGetValue("RemoteControlPassword",out string password))
			{
				httpPassword = password;
			}
			else
			{
				httpPassword = ((DateTime.Now.Millisecond * 49999 + 3067) % 9000 + 999).ToString();
				ModsManager.SetConfig("RemoteControlPassword",httpPassword);
			}
			if(ModsManager.Configs.TryGetValue("RemoteControlEnabled",out string enable)
				&& bool.Parse(enable))
			{
				Task.Run(StartHttpListener);
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
				Window.Frame += delegate {
					frameHandlers.ForEach(function =>
					{
						Invoke(function);
					});
				};
			}
			handlersDictionary = [];
			loader = (JsModLoader)ModsManager.ModLoaders.Find((item) => item is JsModLoader);
			GetAndRegisterHandlers("OnMinerDig");
			GetAndRegisterHandlers("OnMinerPlace");
			GetAndRegisterHandlers("OnPlayerSpawned");
			GetAndRegisterHandlers("OnPlayerDead");
			GetAndRegisterHandlers("ProcessAttackment");
			GetAndRegisterHandlers("CalculateCreatureInjuryAmount");
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
		public static void Execute(Prepared<Script> script)
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
		public static string Evaluate(Prepared<Script> script)
		{
			try
			{
				return engine.Evaluate(script).ToString();
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
				Log.Error(ex);
			}
		}

		public static void SetHttpPort(int port, bool updateConfig = false) {
			httpPort = port;
#if DEBUG
			httpListener.Prefixes.Add("http://+:28256/");
#elif RELEASE
			httpListener.Prefixes.Clear();
			httpListener.Prefixes.Add($"http://{IPAddress.Loopback}:{port}/");
			httpListener.Prefixes.Add($"http://localhost:{port}/");
#endif
			if (updateConfig) {
				ModsManager.SetConfig("RemoteControlPort", port.ToString());
			}
		}
		public static async Task StartHttpListener() {
			try {
				httpListener.Start();
			}
			catch (Exception e) {
				Log.Error("Remote control server starts failed: " + e);
			}
			while (httpListener.IsListening) {
				var context = await httpListener.GetContextAsync();
				Task.Run(() => HandleHttpRequest(context));
			}
		}
		public static void StopHttpListener() {
			//确实能关掉，但有报错，原因不明
			try {
				httpListener.Stop();
			}
			catch{}
		}

		public static void Update() {
			if (httpProcessing & httpScriptPrepared) {
				Stopwatch stopwatch = Stopwatch.StartNew();
				string result = Evaluate(httpScript);
				stopwatch.Stop();
				httpResponse.SetResult(new HttpResponse() {
					success = !result.StartsWith("Jint.Runtime.JavaScriptException"),
					result = result,
					timeCosted = stopwatch.Elapsed
				});
			}
		}

		public static async void HandleHttpRequest(HttpListenerContext context) {
			try {
				string responseString;
				if (httpProcessing) {
					responseString = ErrorJsonResponse(LanguageControl.Get("JsInterface", "1"));
				}
				else if (context.Request.HttpMethod == "POST") {
					if (httpPassword.Length == 0
						|| (context.Request.Headers.Get("password")?.Equals(httpPassword) ?? false)) {
							httpProcessing = true;
							httpScriptPrepared = false;
							httpResponse = new TaskCompletionSource<HttpResponse>();
							try {
								using (Stream bodyStream = context.Request.InputStream) {
									using (StreamReader reader = new(bodyStream, context.Request.ContentEncoding)) {
										string requestBody = reader.ReadToEnd();
										if (requestBody.Length > 0) {
											httpScript = JsEngine.PrepareScript(requestBody);
											httpScriptPrepared = true;
											responseString = JsonSerializer.Serialize(await httpResponse.Task);
										}
										else {
											responseString = ErrorJsonResponse(LanguageControl.Get("JsInterface", "2"));
										}
									}
								}
							}
							catch (Exception e) {
								responseString = ErrorJsonResponse(e.ToString());
							}
							httpProcessing = false;

					}
					else {
						responseString = ErrorJsonResponse(LanguageControl.Get("JsInterface", "3"));
					}
				}
				else if(context.Request.HttpMethod == "ELEVATE")
				{
#if !ANDROID
					Hyper_Threading.SetPriority_Process(true);
#endif
					responseString = "Sucess";
				}
				else
					{
					responseString = ErrorJsonResponse(LanguageControl.Get("JsInterface", "4"));
				}
				HttpListenerResponse response = context.Response;
				response.ContentType = "application/json";
				byte[] buffer = Encoding.UTF8.GetBytes(responseString);
				response.ContentLength64 = buffer.Length;
				Stream output = response.OutputStream;
				await output.WriteAsync(buffer);
				output.Close();
			}
			catch (Exception e) {
				context.Response.Close();
				Log.Error(e);
			}
		}

		public static string ErrorJsonResponse(string error) {
			return JsonSerializer.Serialize(new HttpResponse() {
				success = false,
				result = error,
				timeCosted = TimeSpan.Zero
			});
		}
		public class HttpResponse {
			public bool success { get; set; }
			public string result { get; set; }
			public TimeSpan timeCosted { get; set; }
		}
	}
}