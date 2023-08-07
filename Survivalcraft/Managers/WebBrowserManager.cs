using Engine;
using System;

namespace Game
{
	public static class WebBrowserManager
	{
		public static void LaunchBrowser(string url)
		{

			if (!url.Contains("://"))
			{
				url = "http://" + url;
			}
			try
			{
#if desktop
				System.Diagnostics.Process.Start(url);
#endif
#if android
				Engine.Window.Activity.OpenLink(url);
#endif
			}
			catch (Exception ex)
			{
				Log.Error(string.Format("Error launching web browser with URL \"{0}\". Reason: {1}", new object[2]
				{
					url,
					ex.Message
				}));
			}
		}
	}
}
