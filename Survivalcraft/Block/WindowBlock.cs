namespace Game
{
	public class WindowBlock : AlphaTestCubeBlock
	{
		public const int Index = 60;
        public override bool IsNonAttachable(int value)
        {
            return false;
        }
    }
}
