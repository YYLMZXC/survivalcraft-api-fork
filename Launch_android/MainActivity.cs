using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Engine;
using Game;
using System;
using System.IO;
namespace SC.Android
{
	[Activity(Label = "生存战争2.3插件版", LaunchMode = LaunchMode.SingleTask, Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	[IntentFilter(new string[] { "android.intent.action.VIEW" }, DataScheme = "com.candy.survivalcraft", Categories = new string[] { "android.intent.category.DEFAULT", "android.intent.category.BROWSABLE" })]

	public class MainActivity : EngineActivity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			try
			{
				if (CheckSelfPermission(Manifest.Permission.WriteExternalStorage) != Permission.Granted)
				{
					Toast.MakeText(this, "请授权游戏存储读写权限", ToastLength.Long).Show();
					RequestPermissions(new string[] { Manifest.Permission.WriteExternalStorage }, 0);
				}
				else
				{
					Run();
				}
			}
			catch
			{
				Run();
			}
		}
		public void Run()
		{
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
			
			
			Program.Main();
		}
		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
		{
			bool flag = true;
			foreach (var g in grantResults)
			{
				if (g != Permission.Granted) { flag = false; break; }
			}
			if (flag) Run();
		}
	}
}