using Engine;
using Engine.Graphics;

namespace Game.IContentReader
{
	public class SubtextureReader : IContentReader
	{
		public string[] DefaultSuffix => new string[] { "png", "txt" };
		public string Type => "Game.Subtexture";
		public object Get(ContentInfo[] contents)
		{
			if (contents[0].ContentPath.Contains("Textures/Atlas/"))
			{
				return TextureAtlasManager.GetSubtexture(contents[0].ContentPath);
			}
			else return new Subtexture(ContentManager.Get<Texture2D>(contents[0].ContentPath), Vector2.Zero, Vector2.One);
		}
	}
}
