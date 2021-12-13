using System.IO;
using Engine.Audio;

namespace Game.IContentReader
{
    public class SoundBufferReader:IContentReader
    {
        public override string Type => "Engine.Audio.SoundBuffer";
        public override string[] DefaultSuffix => new string[] { "wav","ogg" };
        public override object Get(ContentInfo[] contents)
        {
            Stream stream = contents[0].Duplicate();
            string f = "";
#if android
            f = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/cache/" + contents[0].Filename;
            Stream file = null;
            if (!File.Exists(f)){
                file = File.Create(f);
                stream.CopyTo(file);
                file.Close();
            }

            stream.Position = 0L;
#endif
            SoundBuffer buffer = SoundBuffer.Load(stream);
            buffer.cachePath = f;
            return buffer;
        }
    }
}
