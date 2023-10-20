using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Permission = Android.Content.PM.Permission;
using Engine;
using Game;
using System;
using System.IO;
using System.Threading.Tasks;
using Android.Provider;
using Android.Widget;
using Xamarin.Essentials;
using Permissions = Xamarin.Essentials.Permissions;
namespace SC4Android
{
	[Activity(Label = "生存战争2.3插件版", LaunchMode = LaunchMode.SingleTask, Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	[IntentFilter(new string[] { "android.intent.action.VIEW" }, DataScheme = "com.candy.survivalcraft", Categories = new string[] { "android.intent.category.DEFAULT", "android.intent.category.BROWSABLE" })]

	public class MainActivity : EngineActivity
	{
		protected override async void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			try
			{
				if ((Convert.ToInt32(Build.VERSION.Release) >= 11) && !Android.OS.Environment.IsExternalStorageManager)
				{
					Toast.MakeText(this, "Android11以上无法直接访问\n需要手动授权，请在稍后的页面中 选择Android/data目录并点击\"选择\"按钮", ToastLength.Long).Show();
					var intent = new Intent(Intent.ActionOpenDocumentTree);
					intent.AddFlags(ActivityFlags.GrantWriteUriPermission);
					StartActivity(intent);
					if ((int)Build.VERSION.SdkInt >= (int)BuildVersionCodes.O)
					{
						await Task.Run(() => intent.PutExtra(DocumentsContract.ExtraInitialUri, "/sdcard/"));
					}
				}
				var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
				if (status == PermissionStatus.Granted)
				{
					return;
				}
				Toast.MakeText(this, "权限需要\", \"此软件需要权限以更改Config文件", ToastLength.Long).Show();
				var request = await Permissions.RequestAsync<Permissions.StorageWrite>();

				if (request == PermissionStatus.Granted)
				{
					return;
				}
				Toast.MakeText(this, "请到应用设置里授权：允许\"读取本机存储\"", ToastLength.Long).Show();
				throw new Exception("应用权限不足");
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