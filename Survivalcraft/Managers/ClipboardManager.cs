#if __IOS__

using UIKit;

namespace Game
{
    public static class ClipboardManager
    {
        public static string ClipboardString
        {
            get
            {
                var pasteboard = UIPasteboard.General;
                return pasteboard.String;

            }
            set
            {
                var pasteboard = UIPasteboard.General;
                pasteboard.String = value;
            }
        }
    }
}

#else
using System.Windows.Forms;

namespace Game
{
    public static class ClipboardManager
    {
        public static string ClipboardString
        {
            get
            {
                return Clipboard.GetText();
            }
            set
            {
                Clipboard.SetText(value);
            }
        }
    }
}

#endif
