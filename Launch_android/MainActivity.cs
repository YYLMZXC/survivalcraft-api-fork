using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Permission = Android.Content.PM.Permission;
using Engine;
using Game;
using Android.Provider;
using Android;
using Environment = Android.OS.Environment;
namespace SC4Android
{
	[Activity(Label = "生存战争2.3插件版", LaunchMode = LaunchMode.SingleTask, Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	[IntentFilter(["android.intent.action.VIEW"], DataScheme = "com.candy.survivalcraft", Categories = ["android.intent.category.DEFAULT", "android.intent.category.BROWSABLE"])]

	public class MainActivity : EngineActivity
	{
		private void CheckAndRequestPermissions()
		{
			if (((int)Build.VERSION.SdkInt) >= (int)BuildVersionCodes.R)
			{
				//当版本大于安卓11时
				if (!Environment.IsExternalStorageManager)
				{
					StartActivity(new Intent(Settings.ActionManageAllFilesAccessPermission));
				}
			}
			else if ((int)Build.VERSION.SdkInt >= (int)BuildVersionCodes.M)
			{
				//当版本大于安卓6
				var readPermissionStatus = CheckSelfPermission(Manifest.Permission.ReadExternalStorage);
				if (readPermissionStatus != Permission.Granted)
				{
					RequestPermissions(new string[] { Manifest.Permission.ReadExternalStorage }, 0);
				}

				var writePermissionStatus = CheckSelfPermission(Manifest.Permission.WriteExternalStorage);
				if (writePermissionStatus != Permission.Granted)
				{
					RequestPermissions(new string[] { Manifest.Permission.WriteExternalStorage }, 1);
				}
			}
		}
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			CheckAndRequestPermissions();
			Run();
		}
		public void Run()
		{
			string[] flist = Assets.List("");
			BasePath = new StreamReader(Assets.Open("apppath.txt")).ReadToEnd();
			ConfigPath = GetExternalFilesDir("").AbsolutePath;
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