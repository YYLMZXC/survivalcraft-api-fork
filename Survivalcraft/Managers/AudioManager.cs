using Engine;
using Engine.Audio;
using System;
using Engine.Media;
namespace Game
{
    public static class AudioManager
    {
        public static float MinAudibleVolume => 0.05f * SettingsManager.SoundsVolume;

        public static void PlaySound(string name, float volume, float pitch, float pan)
        {
            if (SettingsManager.SoundsVolume > 0f)
            {
                float num = volume * SettingsManager.SoundsVolume;
                if (num > MinAudibleVolume)
                {
                    try
                    {
#if android
                        Sound sound = new Sound(ContentManager.Get<StreamingSource>(name, ".wav").Duplicate(), num, ToEnginePitch(pitch), pan, false, true);
                        sound.Play();
#else
                        new Sound(ContentManager.Get<SoundBuffer>(name, ".wav"), num, ToEnginePitch(pitch), pan, isLooped: false, disposeOnStop: true).Play();
#endif
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public static float ToEnginePitch(float pitch)
        {
            return MathUtils.Pow(2f, pitch);
        }
    }
}
