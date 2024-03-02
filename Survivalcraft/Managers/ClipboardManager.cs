using System;
using Game.Handlers;

namespace Game
{
    public static class ClipboardManager
    {
	    public static IClipboardHandler? ClipboardManagerHandler
	    {
		    get;
		    set;
	    }
        private static string HandlerNotInitializedExceptionString
            => $"{typeof(ClipboardManager).FullName}.{nameof(ClipboardManagerHandler)} 未初始化";
        
        public static string ClipboardString
        {
            get
            {
                if (ClipboardManagerHandler is null)
                {
                    throw new Exception(HandlerNotInitializedExceptionString);
                }
                return ClipboardManagerHandler.Text;
            }
            set
            {
                if (ClipboardManagerHandler is null)
                {
                    throw new InvalidOperationException(
                        "剪贴板接受/处理器未定义，请重写 Game.Handlers.IClipboardHandler 并对 Game.ClipboardManager.Handler 赋值");
                }
                ClipboardManagerHandler.Text = value;
            }
        }
    }
}