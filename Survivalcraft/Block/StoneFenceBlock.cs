using Engine;

namespace Game
{
    public class StoneFenceBlock : FenceBlock
    {
        public const int Index = 202;

        public StoneFenceBlock()
            : base("Models/StoneFence", doubleSidedPlanks: false, useAlphaTest: false, 24, new Color(212, 212, 212), Color.White)
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
