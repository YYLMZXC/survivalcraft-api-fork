using System;
using Game.Handlers;

namespace Game
{
    public static class ClipboardManager
    {
	    public static IClipboardHandler? ClipboardManagerServicesCollection
	    {
		    get;
		    set;
	    }
        private static string HandlerNotInitializedExceptionString
            => $"{typeof(ClipboardManager).FullName}.{nameof(ClipboardManagerServicesCollection)} 未初始化";
        
        public static string ClipboardString
        {
            get
            {
                if (ClipboardManagerServicesCollection is null)
                {
                    throw new Exception(HandlerNotInitializedExceptionString);
                }
                return ClipboardManagerServicesCollection.Text;
            }
            set
            {
                if (ClipboardManagerServicesCollection is null)
                {
                    throw new InvalidOperationException(
                        "剪贴板接受/处理器未定义，请重写 Game.Handlers.IClipboardHandler 并对 Game.ClipboardManager.Handler 赋值");
                }
                ClipboardManagerServicesCollection.Text = value;
            }
        }
    }
}