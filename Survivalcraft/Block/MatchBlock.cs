namespace Game
{
	public class MatchBlock : FlatBlock
	{
		public const int Index = 108;
        public override int GetPriorityUse(int value, ComponentMiner componentMiner)
        {
            return 1;
        }
    }
}
