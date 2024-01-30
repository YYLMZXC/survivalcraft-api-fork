using Engine.Graphics;

namespace Game.IContentReader
{
	public class DaeModelReader : IContentReader
	{
		public string Type => "Engine.Graphics.Model";
		public string[] DefaultSuffix => new string[] { "dae" };
		public object Get(ContentInfo[] contents)
		{
			return Model.Load(contents[0].Duplicate(), true);
		}
	}
}
