using Engine;

namespace Game
{
    public class DirtBlock : CubeBlock
    {
        public const int Index = 2;
        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
        {
            generator.GenerateCubeVertices(this, value, x, y, z, Color.White, geometry.OpaqueSubsetsByFace);
        }
        public override BlockPlacementData GetDigValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, int toolValue, TerrainRaycastResult raycastResult)
        {
            componentMiner.Inventory.AddSlotItems(4, 18, 1);
            return base.GetDigValue(subsystemTerrain, componentMiner, value, toolValue, raycastResult);
        }
    }
}
