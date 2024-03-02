using Engine;
using Game.Handlers;

namespace Game
{
	public class HardwareManager
	{
		public static IHardwareManagerHandler? HardwareManagerServicesCollection
		{
			get;
			set;
		}
		
		private static string HandlerNotInitializedErrorString
			=> $"{typeof(HardwareManager).FullName}.{nameof(HardwareManagerServicesCollection)} 未初始化";
		
		public void Vibrate(long ms)
		{
			if (HardwareManagerServicesCollection is null)
			{
				Log.Error(HandlerNotInitializedErrorString);
				return;
			}
			HardwareManagerServicesCollection.Vibrate(ms);
		}
	}
}
