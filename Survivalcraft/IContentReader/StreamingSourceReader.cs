using Engine.Media;
namespace Game.IContentReader
{
	public class StreamingSourceReader : IContentReader
	{
		public string Type => "Engine.Media.StreamingSource";
		public string[] DefaultSuffix => new string[] { "wav", "ogg" };
		public object Get(ContentInfo[] contents)
		{
			return SoundData.Stream(contents[0].Duplicate());
		}
	}
}
