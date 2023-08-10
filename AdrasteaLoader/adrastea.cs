namespace Game
{
	public class AdrasteaLoader
	{
	
		public string 点击方块的部位(int part)
		{
			switch (part)
			{
				case 0:
					return "click_uppart";
				case 1:
					return "click_downpart";
				case 2:
					return "click_leftpart";
				case 3: 
					return"click_rightpart ";
				default:
					return null;
			}
			}
	}
	public class SoarLib_UNIpowerAPI
	{
		
	}
}
