using Engine;

namespace Game
{
	public static class ClipboardManager
	{
		public static string ClipboardString
		{
			get
			{
				return ((Android.Content.ClipboardManager)Window.Activity.GetSystemService("clipboard")).Text;
			}
			set
			{
				((Android.Content.ClipboardManager)Window.Activity.GetSystemService("clipboard")).Text = value;
			}
		}
	}
}