namespace Game
{
	public class SandBlock : CubeBlock
	{
		public const int Index = 7;
        public override bool IsSuitableForPlants(int value, int plantValue)
        {
            int plantContent = Terrain.ExtractContents(plantValue);
            Block plantBlock = BlocksManager.Blocks[plantContent];
            if (plantBlock is SaplingBlock)
            {
                return false;
            }
            return true;
        }
    }
}
