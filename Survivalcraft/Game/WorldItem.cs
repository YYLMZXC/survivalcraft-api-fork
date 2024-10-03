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

		public IInventory Creator;//ָ���õ�����������Ǵ���һ��IInventory���ɵġ�Ŀǰ����û�����ơ�����ʵ�ַ�������������mod������ͼ�ٻ��������Ȳ�����

		public bool IsFireProof = false;//�õ�����͵�������𣬲��ᱻ����������ջ�

        public float? MaxTimeExist;

        public float ExplosionMass = 20f;
		public virtual void UnderExplosion(Vector3 impulse, float damage) { }
    }
}
