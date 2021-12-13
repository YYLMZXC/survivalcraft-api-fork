using System;
using Android.Media;
using System.Collections.Generic;

namespace Engine.Audio
{
    public sealed class Sound : BaseSound
    {
        public class SoundItem
        {
            public SoundPool soundPool;
            public int playID;
        }

        public bool m_audioTrackCreateAttempted;

        public SoundBuffer m_soundBuffer;

        public SoundBuffer SoundBuffer => m_soundBuffer;

        public SoundPool soundPool = null;

        public bool loaded = false;

        public int SoundID = 0;

        public int playID = 0;

        public int StreamID = 0;

        public bool play = false;

        public static Dictionary<string, int> SoundMaps = new Dictionary<string, int>();

        public static Dictionary<string, SoundItem> LoadedMaps = new Dictionary<string, SoundItem>();

        public Sound(SoundBuffer soundBuffer, float volume = 1f, float pitch = 1f, float pan = 0f, bool isLooped = false, bool disposeOnStop = false)
        {
            if (soundBuffer == null)
            {
                throw new ArgumentNullException("soundBuffer");
            }
            ChannelsCount = soundBuffer.ChannelsCount;
            SamplingFrequency = soundBuffer.SamplingFrequency;
            Volume = volume;
            Pitch = pitch;
            Pan = pan;
            IsLooped = isLooped;
            DisposeOnStop = disposeOnStop;
            //m_stopPosition = (m_isLooped ? (-1) : (SoundBuffer.SamplesCount - 1));
            if (!SoundMaps.TryGetValue(soundBuffer.cachePath, out SoundID))
            {
                soundPool = new SoundPool(15, Stream.Music, 0);
                SoundID = soundPool.Load(soundBuffer.cachePath, 1);
                SoundMaps.Add(soundBuffer.cachePath, SoundID);
                System.Diagnostics.Debug.WriteLine("Path: " + soundBuffer.cachePath + " GET SoundID:" + SoundID);
                soundPool.LoadComplete += (obj, arg) => {
                    playID = arg.SampleId;
                    loaded = true;
                    State = SoundState.Stopped;
                    if (!LoadedMaps.TryGetValue(soundBuffer.cachePath, out var item))
                    {
                        LoadedMaps.Add(soundBuffer.cachePath, new SoundItem() { soundPool = soundPool, playID = playID });
                    }
                    if (play)
                    {
                        StreamID = soundPool.Play(playID, Volume, Volume, 1, 0, 1.0f);
                    }
                };
            }
            if (LoadedMaps.TryGetValue(soundBuffer.cachePath, out var item))
            {
                playID = item.playID;
                soundPool = item.soundPool;
                loaded = true;
                State = SoundState.Stopped;
            }
        }

        internal override void InternalPlay()
        {
            play = true;
            if (loaded)
            {
                StreamID = soundPool.Play(playID, Volume, Volume, 1, 0, 1.0f);
            }
        }

        internal override void InternalPause()
        {
            soundPool.Pause(StreamID);
        }

        internal override void InternalStop()
        {
            soundPool.Stop(StreamID);
        }

        internal override void InternalDispose()
        {
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        internal void Initialize(SoundBuffer soundBuffer)
        {
            if (soundBuffer == null)
            {
                throw new ArgumentNullException("soundBuffer");
            }
            m_soundBuffer = soundBuffer;
            int num = ++m_soundBuffer.UseCount;
        }
    }
}
