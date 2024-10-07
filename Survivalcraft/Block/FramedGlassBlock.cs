namespace Game
{
	public class FramedGlassBlock : AlphaTestCubeBlock
	{
		public const int Index = 44;
		public override bool ShouldGenerateFace(SubsystemTerrain subsystemTerrain, int face, int value, int neighborValue)
		{
			if (Terrain.ExtractContents(neighborValue) == BlockIndex) return false;
			return base.ShouldGenerateFace(subsystemTerrain, face, value, neighborValue);
		}
        public override bool IsNonAttachable(int value)
        {
            return false;
        }
    }
}
