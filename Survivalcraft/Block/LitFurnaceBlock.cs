namespace Game
{
	public class LitFurnaceBlock : FurnaceBlock
	{
		public new const int Index = 65;
        public override bool IsMovableByPiston(int value, int pistonFace, int y, out bool isEnd)
        {
            isEnd = false;
            return false;
        }
    }
}
