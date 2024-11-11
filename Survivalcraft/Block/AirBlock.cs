﻿using Engine;
using Engine.Graphics;

namespace Game
{
	public class AirBlock : Block
	{
		public static int Index = 0;

		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
            if(Terrain.ExtractContents(value) != 0) 
				BlocksManager.DrawFlatOrImageExtrusionBlock(primitivesRenderer, 111, size, ref matrix, null, color, isEmissive: false, environmentData);
        }

		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
		{
            generator.GenerateCubeVertices(this, 111, x, y, z, Color.Magenta, geometry.OpaqueSubsetsByFace);
        }

        public override IEnumerable<int> GetCreativeValues()
        {
            yield break;
        }

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
        {
			int content = Terrain.ExtractContents(value);
            if(content == 0) return base.GetDisplayName(subsystemTerrain, value);
			return ("未知方块" + value);
        }
    }
}
