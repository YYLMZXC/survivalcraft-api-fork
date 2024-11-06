using Engine.Media;

namespace Game.IContentReader
{
	public class ImageReader : IContentReader
	{
		public override string Type => "Engine.Media.Image";
		public override string[] DefaultSuffix => ["webp", "png", "jpg", "jpeg"];
		public override object Get(ContentInfo[] contents)
		{
			return Image.Load(contents[0].Duplicate());
		}

	}
}
