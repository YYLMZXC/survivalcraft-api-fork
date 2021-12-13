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
                        SoundBuffer sound = ContentManager.Get<SoundBuffer>(name);
                        new Sound(sound, 1f, ToEnginePitch(pitch), pan, false, true).Play();
                    }
                    catch (Exception e)
                    {
                        Log.Error(e.Message);
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
