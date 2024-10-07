using Engine;

namespace Game
{
	public class BrickFenceBlock : FenceBlock
	{
		public const int Index = 164;

		public BrickFenceBlock()
			: base("Models/StoneFence", doubleSidedPlanks: false, useAlphaTest: false, 39, new Color(212, 212, 212), Color.White)
		{
		}

		public override bool ShouldConnectTo(int value)
		{
			if (BlocksManager.Blocks[Terrain.ExtractContents(value)].IsNonAttachable(value))
			{
				return base.ShouldConnectTo(value);
			}
			return true;
		}
	}
}
