using Engine;
using Engine.Graphics;

namespace Game
{
	public class AirBlock : Block
	{
		public const int Index = 0;

		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
            if(Terrain.ExtractContents(value) != 0) 
				BlocksManager.DrawFlatOrImageExtrusionBlock(primitivesRenderer, 193, size, ref matrix, null, color, isEmissive: false, environmentData);
        }

		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
		{
		}

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
        {
			int content = Terrain.ExtractContents(value);
            if(content == 0) return base.GetDisplayName(subsystemTerrain, value);
			return ("Î´Öª·½¿é" + value);
        }
    }
}
