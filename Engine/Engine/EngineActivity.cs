#if ANDROID
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace Engine
{

    public class EngineActivity : Activity
    {
        internal static EngineActivity m_activity;

        public event Action Paused;

        public event Action Resumed;

        public event Action Destroyed;

        public event Action<Intent> NewIntent;

        public const string ExtPath = "";
        public const string DocPath = "config:";

        public EngineActivity()
        {
            m_activity = this;
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.AddFlags(WindowManagerFlags.Fullscreen);
            VolumeControlStream = Android.Media.Stream.Music;
            RequestedOrientation = ScreenOrientation.SensorLandscape;
        }

        public void Vibrate(long ms)
        {
            Vibrator vibrator = (Vibrator)GetSystemService("vibrator");
            vibrator.Vibrate(VibrationEffect.CreateOneShot(1000, VibrationEffect.DefaultAmplitude));
        }
        public void OpenLink(string link)
        {

            Intent intent = new();

            intent.SetAction("android.intent.action.VIEW");

            Android.Net.Uri content_url = Android.Net.Uri.Parse(link);

            intent.SetData(content_url);

            intent.SetClassName("com.android.browser", "com.android.browser.BrowserActivity");

            StartActivity(intent);
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
#endif