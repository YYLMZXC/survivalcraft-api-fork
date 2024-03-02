using Engine;
using System;
using Game.Handlers;

namespace Game
{
	public static class WebBrowserManager
	{
		public static IWebBrowserManagerHandler? WebBrowserManagerServicesCollection
		{
			get;
			set;
		}
		private static string HandlerNotInitializedWarningString
			=> $"{typeof(WebBrowserManager).FullName}.{nameof(WebBrowserManagerServicesCollection)} 未初始化";
		public static void LaunchBrowser(string url)
		{

			if (!url.Contains("://"))
			{
				url = "http://" + url;
			}
			try
			{
				if (WebBrowserManagerServicesCollection is null)
				{
					Log.Warning(HandlerNotInitializedWarningString);
					return;
				}
				WebBrowserManagerServicesCollection.LaunchBrowser(url);
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
