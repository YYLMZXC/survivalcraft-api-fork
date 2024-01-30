using Engine.Audio;

namespace Game.IContentReader
{
	public class SoundBufferReader : IContentReader
	{
		public string Type => "Engine.Audio.SoundBuffer";
		public string[] DefaultSuffix => new string[] { "wav", "ogg" };
		public object Get(ContentInfo[] contents)
		{
			return SoundBuffer.Load(contents[0].Duplicate());
		}
	}
}
