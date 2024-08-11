using Engine;

namespace Game
{
	public class WorldItem
	{
		public int Value;

		public Vector3 Position;

		public Vector3 Velocity;

		public double CreationTime;

		public int Light;

		public bool ToRemove;

		public IInventory Creator;//指明该弹射物，掉落物是从哪一个IInventory生成的。目前这里没有完善。可以实现发射器攻击会让mod生物试图毁坏发射器等操作。
	}
}
