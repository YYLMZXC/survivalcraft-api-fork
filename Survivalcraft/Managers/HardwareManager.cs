using Engine;
using Game.Handlers;

namespace Game
{
	public class HardwareManager
	{
		public static IHardwareManagerHandler? HardwareManagerHandler
		{
			get;
			set;
		}
		
		private static string HandlerNotInitializedErrorString
			=> $"{typeof(HardwareManager).FullName}.{nameof(HardwareManagerHandler)} 未初始化";
		
		public void Vibrate(long ms)
		{
			if (HardwareManagerHandler is null)
			{
				Log.Error(HandlerNotInitializedErrorString);
				return;
			}
			HardwareManagerHandler.Vibrate(ms);
		}
	}
}
