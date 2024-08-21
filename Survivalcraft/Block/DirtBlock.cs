namespace Game
{
	public class DirtBlock : CubeBlock
	{
		public const int Index = 2;

        public override bool IsPlantableBlock(int value, int plantValue)
        {
            return true;
        }
    }
}
