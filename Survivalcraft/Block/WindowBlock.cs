namespace Game
{
	public class WindowBlock : AlphaTestCubeBlock
	{
		public static int Index = 60;
        public override bool IsNonAttachable(int value)
        {
            return false;
        }
    }
}
