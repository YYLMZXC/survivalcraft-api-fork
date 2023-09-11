using Foundation;
using GameController;
using OpenTK.Platform.iPhoneOS;
using survivalcraftAPI;
using UIKit;

namespace SurvivalCraftAPI
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations

        public override UIWindow Window
        {
            get;
            set;
        }
        public GameViewController Controller;

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            Window = new UIWindow();
            Controller = new GameViewController();
            Window.RootViewController = Controller;
            Window.MakeKeyAndVisible();

            return true;
        }
        public override bool HandleOpenURL(UIApplication application, NSUrl url)
        {
            return base.HandleOpenURL(application, url);
        }
        public override void DidEnterBackground(UIApplication application)
        {
            Controller.EnterBackground();
        }

        public override void WillEnterForeground(UIApplication application)
        {
            Controller.EnterForeground();
        }
        public override void WillTerminate(UIApplication application)
        {
            Controller.OnExit();
        }
    }
}


