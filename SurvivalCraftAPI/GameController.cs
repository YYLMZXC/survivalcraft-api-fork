using CoreGraphics;
using Engine;
using Engine.Graphics;
using Foundation;
using GLKit;
using OpenGLES;
using System.Text;
using UIKit;
using static CoreMedia.CMTime;

namespace survivalcraftAPI
{
    public class GameViewController : GLKViewController
    {
        private GLKView gLKView;
        StringBuilder builder = new StringBuilder();
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            gLKView = (GLKView)View;
            gLKView.Context = new EAGLContext(EAGLRenderingAPI.OpenGLES2);
            EAGLContext.SetCurrentContext(gLKView.Context);
            Window.UIView = gLKView;
            Window.uIViewController = this;
            var bounds = UIScreen.MainScreen.Bounds;
            float scale = (float)UIScreen.MainScreen.Scale;
            Window.PixelScale = (int)scale;
            Window.ScreenSize = new Point2((int)bounds.Size.Width, (int)bounds.Size.Height);
            Window.Size = Window.ScreenSize * Window.PixelScale;
            Window.LoadHandler();
            Game.Program.Main();
        }
        public override void Update()
        {
            Window.RenderFrameHandler();
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            return UIInterfaceOrientationMask.Landscape;
        }
        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            Engine.Input.Touch.HandleTouchEvent(gLKView, touches, Engine.Input.Touch.MotionEventActions.Down);
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);
            Engine.Input.Touch.HandleTouchEvent(gLKView, touches, Engine.Input.Touch.MotionEventActions.Move);
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
            Engine.Input.Touch.HandleTouchEvent(gLKView, touches, Engine.Input.Touch.MotionEventActions.Up);
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);

            // 在触摸取消时执行操作
        }
        /// <summary>
        /// 程序进入焦点
        /// </summary>
        public void EnterForeground()
        {
            Window.FocusedChangedHandler(true);
        }
        /// <summary>
        /// 程序失去焦点
        /// </summary>
        public void EnterBackground()
        {
            Window.FocusedChangedHandler(false);
        }
        /// <summary>
        /// 程序退出
        /// </summary>
        public void OnExit()
        {
            Window.ClosedHandler();
        }
    }
}

