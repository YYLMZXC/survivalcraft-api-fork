using System.Collections.Generic;

namespace Game
{
	public class SaplingBlock : CrossBlock
	{
		public const int Index = 119;

		public override int GetFaceTextureSlot(int face, int value)
		{
			switch (Terrain.ExtractData(value))
			{
				case 0:
					return 56;
				case 1:
					return 72;
				case 2:
					return 73;
				case 3:
					return 73;
				case 4:
					return 72;
				default:
					return 56;
			}
		}

		public override IEnumerable<int> GetCreativeValues()
		{
			yield return Terrain.MakeBlockValue(119, 0, 0);
			yield return Terrain.MakeBlockValue(119, 0, 1);
			yield return Terrain.MakeBlockValue(119, 0, 2);
			yield return Terrain.MakeBlockValue(119, 0, 3);
			yield return Terrain.MakeBlockValue(119, 0, 4);
		}
	}
}
