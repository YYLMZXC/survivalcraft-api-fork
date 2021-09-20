using Engine.Graphics;
using Engine;
using System.IO;

namespace Game.IContentReader
{
    public class SubtextureReader:IContentReader
    {
        public override string[] DefaultSuffix => new string[] { "png", "txt" };
        public override string Type => "Game.Subtexture";
        public override object Get(ContentInfo[] contents)
        {
            if (contents[0].ContentPath.Contains("Textures/Atlas/"))
            {
                return TextureAtlasManager.GetSubtexture(contents[0].ContentPath);
            }
            else return new Subtexture(ContentManager.Get<Texture2D>(contents[0].ContentPath), Vector2.Zero, Vector2.One);
        }
    }
}
