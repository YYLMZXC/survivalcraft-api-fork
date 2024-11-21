using Engine;

namespace Game
{
	public struct MovingBlocksRaycastResult
	{
		public Ray3 Ray;

		public IMovingBlockSet MovingBlockSet;

		public float Distance;
		public Vector3 HitPoint()
		{
			return Ray.Position + (Ray.Direction * Distance);
		}

		public MovingBlock MovingBlock;

		public int CollisionBoxIndex;

		public BoundingBox? BlockBoundingBox;

		public int BlockValue
		{
			get
			{
				return MovingBlock?.Value ?? -1;
			}
		}
	}
}
