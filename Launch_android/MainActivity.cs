using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Engine;
using Game;
namespace SC.Android
{
    [Activity(Label = "生存战争2.2插件版", LaunchMode = LaunchMode.SingleTask, Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    [IntentFilter(new string[] { "android.intent.action.VIEW" }, DataScheme = "com.candy.survivalcraft", Categories = new string[] { "android.intent.category.DEFAULT", "android.intent.category.BROWSABLE" })]

    public class MainActivity : EngineActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Program.Main();  
        }
    }
}