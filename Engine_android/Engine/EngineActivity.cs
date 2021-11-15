using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Net;
using Android.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Engine
{

	public class EngineActivity : Activity
	{
		internal static EngineActivity m_activity;

		public event Action Paused;

		public event Action Resumed;

		public event Action Destroyed;

		public event Action<Intent> NewIntent;

		public static string BasePath = "";

		public static string ConfigPath = "";

		public EngineActivity()
		{
			m_activity = this;
		}
		public void OpenLink(string link)
		{

			Intent intent = new Intent();

			intent.SetAction("android.intent.action.VIEW");

			Android.Net.Uri content_url = Android.Net.Uri.Parse(link);

			intent.SetData(content_url);

			intent.SetClassName("com.android.browser", "com.android.browser.BrowserActivity");

			StartActivity(intent);
		}
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			if (CheckSelfPermission(Manifest.Permission.WriteExternalStorage) != Permission.Granted)
			{
				RequestPermissions(new string[] { Manifest.Permission.WriteExternalStorage }, 0);
			}
			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.Fullscreen);
			VolumeControlStream = Android.Media.Stream.Music;
			RequestedOrientation = ScreenOrientation.SensorLandscape;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			string[] flist = Assets.List("");
			BasePath = new StreamReader(Assets.Open("apppath.txt")).ReadToEnd();
			ConfigPath = this.GetExternalFilesDir("").AbsolutePath;
			foreach (string dll in flist)
			{
				if (dll.EndsWith(".dll"))
				{
					MemoryStream memoryStream = new MemoryStream();
					Assets.Open(dll).CopyTo(memoryStream);
					AppDomain.CurrentDomain.Load(memoryStream.ToArray());
				}
			}
		}

		protected override void OnPause()
		{
			base.OnPause();
			Paused?.Invoke();
		}

		protected override void OnResume()
		{
			base.OnResume();
			Resumed?.Invoke();
		}

		protected override void OnNewIntent(Intent intent)
		{
			base.OnNewIntent(intent);
			NewIntent?.Invoke(intent);
		}

		protected override void OnDestroy()
		{
			try
			{
				base.OnDestroy();
				Destroyed?.Invoke();
			}
			finally
			{
				Thread.Sleep(250);
				System.Environment.Exit(0);
			}
		}
	}
}