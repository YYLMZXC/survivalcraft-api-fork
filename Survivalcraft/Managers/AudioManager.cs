using Engine;
using Engine.Audio;
using System;
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
						SoundBuffer soundBuffer = ContentManager.Get<SoundBuffer>(name, ".flac");
						if (soundBuffer == null) soundBuffer = ContentManager.Get<SoundBuffer>(name, ".wav");
                        if (soundBuffer == null) soundBuffer = ContentManager.Get<SoundBuffer>(name, ".ogg");
                        Sound sound = new(soundBuffer, num, ToEnginePitch(pitch), pan, isLooped: false, disposeOnStop: true);
						sound.Play();
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
