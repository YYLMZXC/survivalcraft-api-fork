using Engine;
using System;
using Game.Handlers;

namespace Game
{
	public static class WebBrowserManager
	{
		public static IWebBrowserManagerHandler? WebBrowserManagerHandler
		{
			get;
			set;
		}
		private static string HandlerNotInitializedWarningString
			=> $"{typeof(WebBrowserManager).FullName}.{nameof(WebBrowserManagerHandler)} 未初始化";
		public static void LaunchBrowser(string url)
		{

			if (!url.Contains("://"))
			{
				url = "http://" + url;
			}
			try
			{
				if (WebBrowserManagerHandler is null)
				{
					Log.Warning(HandlerNotInitializedWarningString);
					return;
				}
				WebBrowserManagerHandler.LaunchBrowser(url);
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
