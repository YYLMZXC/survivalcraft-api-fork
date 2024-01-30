using Engine.Graphics;
namespace Game.IContentReader
{
	public class Texture2DReader : IContentReader
	{
		public string Type => "Engine.Graphics.Texture2D";
		public string[] DefaultSuffix => new string[] { "png", "jpg", "jpeg" };
		public object Get(ContentInfo[] contents)
		{
			return Texture2D.Load(contents[0].Duplicate());
		}
	}
}
