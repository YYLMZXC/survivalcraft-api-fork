namespace Game
{
	public class GlassBlock : AlphaTestCubeBlock
	{
		public const int Index = 15;

		public GlassBlock()
		{
			CanBeBuiltIntoFurniture = true;
		}

		public override bool ShouldGenerateFace(SubsystemTerrain subsystemTerrain, int face, int value, int neighborValue)
		{
			if (Terrain.ExtractContents(neighborValue) == BlockIndex) return false;
			return base.ShouldGenerateFace(subsystemTerrain, face, value, neighborValue);
		}
	}
}
