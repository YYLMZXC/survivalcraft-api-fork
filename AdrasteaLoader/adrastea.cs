namespace Game
{
	public class AdrasteaLoader
	{
	
		public string 点击方块的部位(int part)
		{
			L_点击方块的部位_top:
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
					goto L_点击方块的部位_top;
			}
			}
	}
	public class SoarLib_UNIpowerAPI
	{
		
	}
}
