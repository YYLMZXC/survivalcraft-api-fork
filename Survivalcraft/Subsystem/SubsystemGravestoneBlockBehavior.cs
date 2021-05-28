namespace Game
{
    public class SubsystemGravestoneBlockBehavior : SubsystemBlockBehavior
    {
        public override int[] HandledBlocks => new int[1]
        {
            189
        };

        public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
        {
            int cellValue = base.SubsystemTerrain.Terrain.GetCellValue(x, y - 1, z);
            if (BlocksManager.Blocks[Terrain.ExtractContents(cellValue)].IsTransparent_(cellValue))
            {
                base.SubsystemTerrain.DestroyCell(0, x, y, z, 0, noDrop: false, noParticleSystem: false);
            }
        }
    }
}
