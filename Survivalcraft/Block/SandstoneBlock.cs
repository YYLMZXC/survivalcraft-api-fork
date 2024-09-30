namespace Game
{
	public class SandstoneBlock : PaintedCubeBlock
	{
		public const int Index = 4;

		public SandstoneBlock()
			: base(64)
		{
			CanBeBuiltIntoFurniture = true;
		}
	}
}
