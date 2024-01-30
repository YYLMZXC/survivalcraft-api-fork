using Engine.Media;

namespace Game.IContentReader
{
	public class BitmapFontReader : IContentReader
	{
		public string Type => "Engine.Media.BitmapFont";
		public string[] DefaultSuffix => ["lst", "png"];
		public object Get(ContentInfo[] contents)
		{
			if (contents.Length != 2) throw new System.Exception("not matches content count");
			return BitmapFont.Initialize(contents[1].Duplicate(), contents[0].Duplicate());
		}
	}
}
