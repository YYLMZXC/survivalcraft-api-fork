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
						SoundBuffer soundBuffer = ContentManager.Get<SoundBuffer>(name, ".flac", false);
						if (soundBuffer == null) soundBuffer = ContentManager.Get<SoundBuffer>(name, ".wav", false);
                        if (soundBuffer == null) soundBuffer = ContentManager.Get<SoundBuffer>(name, ".ogg", false);
                        if (soundBuffer == null) soundBuffer = ContentManager.Get<SoundBuffer>(name, ".mp3", true);
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
			return MathF.Pow(2f, pitch);
		}
	}
}
