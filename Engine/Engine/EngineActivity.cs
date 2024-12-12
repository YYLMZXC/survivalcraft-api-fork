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

        public event Func<KeyEvent, bool> OnDispatchKeyEvent;
        
        public static string BasePath = RunPath.AndroidFilePath;
        public static string ConfigPath = RunPath.AndroidFilePath;

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
            vibrator.Vibrate(VibrationEffect.CreateOneShot(ms, VibrationEffect.DefaultAmplitude));
        }
        public void OpenLink(string link)
        {
            StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(link)));
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

        public override bool DispatchKeyEvent(KeyEvent e)
        {
            bool handled = false;
            var invocationList = OnDispatchKeyEvent?.GetInvocationList();
            if (invocationList == null)
            {
                return base.DispatchKeyEvent(e);
            }

            foreach (var invocation in invocationList)
            {
                handled |= (bool)invocation.DynamicInvoke([e])!;
            }

            return handled || base.DispatchKeyEvent(e);
        }
    }
}
#endif
