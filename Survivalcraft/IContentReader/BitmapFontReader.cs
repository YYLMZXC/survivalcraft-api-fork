using Engine;
using Engine.Media;

namespace Game.IContentReader
{
	public class BitmapFontReader : IContentReader
	{
		public override string Type => "Engine.Media.BitmapFont";
		public override string[] DefaultSuffix => ["lst", "webp", "png"];
		public override object Get(ContentInfo[] contents)
		{
			return contents.Length != 2
				? throw new System.Exception("not matches content count")
				: (object)BitmapFont.Initialize(contents[1].Duplicate(), contents[0].Duplicate(), new Vector2(0f, -3f));
		}
	}
}
