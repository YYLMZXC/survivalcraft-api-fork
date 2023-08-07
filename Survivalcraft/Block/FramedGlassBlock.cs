namespace Game
{
	public class FramedGlassBlock : AlphaTestCubeBlock
	{
		public const int Index = 44;
		public override bool ShouldGenerateFace(SubsystemTerrain subsystemTerrain, int face, int value, int neighborValue)
		{
			if (Terrain.ExtractContents(neighborValue) == Index) return false;
			return base.ShouldGenerateFace(subsystemTerrain, face, value, neighborValue);
		}
	}
}
