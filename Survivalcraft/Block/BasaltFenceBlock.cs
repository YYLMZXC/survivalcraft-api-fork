using Engine;

namespace Game
{
	public class BasaltFenceBlock : FenceBlock
	{
		public const int Index = 163;

		public BasaltFenceBlock()
			: base("Models/StoneFence", doubleSidedPlanks: false, useAlphaTest: false, 40, new Color(212, 212, 212), Color.White)
		{
		}

		public override bool ShouldConnectTo(int value)
		{
			if (BlocksManager.Blocks[Terrain.ExtractContents(value)].IsTransparent_(value))
			{
				return base.ShouldConnectTo(value);
			}
			return true;
		}
	}
}
