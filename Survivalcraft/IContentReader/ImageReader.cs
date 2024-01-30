using Engine.Media;

namespace Game.IContentReader
{
	public class ImageReader : IContentReader
	{
		public string Type => "Engine.Media.Image";
		public string[] DefaultSuffix => new string[] { "png", "jpeg", "jpg" };
		public object Get(ContentInfo[] contents)
		{
			return Image.Load(contents[0].Duplicate());
		}

	}
}
