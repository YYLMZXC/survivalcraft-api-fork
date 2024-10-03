using Engine;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemBottomSuckerBlockBehavior : SubsystemInWaterBlockBehavior
	{
		public override int[] HandledBlocks => new int[0];

		public int m_seaUrchinBlockValue;
        public override void Load(ValuesDictionary valuesDictionary)
        {
            base.Load(valuesDictionary);
			m_seaUrchinBlockValue = BlocksManager.GetBlockIndex<SeaUrchinBlock>();
        }
        public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
		{
			base.OnNeighborBlockChanged(x, y, z, neighborX, neighborY, neighborZ);
			int face = BottomSuckerBlock.GetFace(Terrain.ExtractData(SubsystemTerrain.Terrain.GetCellValue(x, y, z)));
			Point3 point = CellFace.FaceToPoint3(CellFace.OppositeFace(face));
			int cellValue = SubsystemTerrain.Terrain.GetCellValue(x + point.X, y + point.Y, z + point.Z);
			if (!IsSupport(cellValue, face))
			{
				SubsystemTerrain.DestroyCell(0, x, y, z, 0, noDrop: false, noParticleSystem: false);
			}
		}

		public override void OnCollide(CellFace cellFace, float velocity, ComponentBody componentBody)
		{
			if (Terrain.ExtractContents(SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z)) == m_seaUrchinBlockValue)
			{
				ComponentHealth componentHealth = componentBody.Entity.FindComponent<ComponentHealth>();
				if (componentHealth != null) {
					componentHealth.OnSpiked(this, 0.1f / componentHealth.SpikeResilience * MathF.Abs(velocity), cellFace, velocity, componentBody, "Spiked by a sea creature");
                }
			}
		}

		public virtual bool IsSupport(int value, int face)
		{
			Block block = BlocksManager.Blocks[Terrain.ExtractContents(value)];
			if (block.IsCollidable_(value))
			{
				return !block.IsFaceTransparent(SubsystemTerrain, CellFace.OppositeFace(face), value);
			}
			return false;
		}
	}
}
