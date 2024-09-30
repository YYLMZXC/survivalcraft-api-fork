namespace Game
{
	public class DiamondBlock : CubeBlock
	{
		public const int Index = 126;

        public DiamondBlock() {
            CanBeBuiltIntoFurniture = true;
        }
        public override bool IsMovableByPiston(int value, int pistonFace, int y, out bool isEnd)
        {
            isEnd = false;
            return false;
        }
    }
}
