using Engine;
namespace Game.Managers
{
	internal class illuminationManager
	{
		public static int 六面光照最大值(SubsystemTerrain terrain,Vector3 position)
		{
			int num = Terrain.ToCell(position.X);
			int num2 = Terrain.ToCell(position.Y);
			int num3 = Terrain.ToCell(position.Z);
			int x = 0;
			x = MathUtils.Max(x,terrain.Terrain.GetCellLight(num + 1,num2,num3));
			x = MathUtils.Max(x,terrain.Terrain.GetCellLight(num - 1,num2,num3));
			x = MathUtils.Max(x,terrain.Terrain.GetCellLight(num,num2 + 1,num3));
			x = MathUtils.Max(x,terrain.Terrain.GetCellLight(num,num2 - 1,num3));
			x = MathUtils.Max(x,terrain.Terrain.GetCellLight(num,num2,num3 + 1));
			x = MathUtils.Max(x,terrain.Terrain.GetCellLight(num,num2,num3 - 1));
			return x;
		}
	}
}
