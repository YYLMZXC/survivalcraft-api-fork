using System.IO;
using Engine.Audio;

namespace Game.IContentReader
{
    public class SoundReader : IContentReader
    {
        public override string Type => "Engine.Audio.Sound";
        public override string[] DefaultSuffix => new string[] { "wav", "ogg" };
        public override object Get(ContentInfo[] contents)
        {
            return new Sound(SoundBuffer.Load(contents[0].Duplicate()));
        }
    }
}
