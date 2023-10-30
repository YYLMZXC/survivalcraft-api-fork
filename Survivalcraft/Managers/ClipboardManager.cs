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
				//return Windows.ApplicationModel.DataTransfer.Clipboard.GetContent().ToString();

            }
			set
			{
				Clipboard.SetText(value);
				//Clipboard.SetContent((C)value);
			}
		}
	}
}
