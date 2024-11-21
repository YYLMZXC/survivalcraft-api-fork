using Acornima.Ast;
using Engine;
using Engine.Audio;
using System;
namespace Game
{
	public static class AudioManager
	{
		public static float MinAudibleVolume => 0.05f * SettingsManager.SoundsVolume;
		public static void PlaySound(string name,float volume,float pitch,float pan)
		{
			PlaySound(name,volume,pitch,pan,Vector3.Zero);
		}
		public static void PlaySound(string name, float volume, float pitch, float pan,Vector3 vector)
		{
			if (SettingsManager.SoundsVolume > 0f)
			{
				float num = volume * SettingsManager.SoundsVolume;
				if (num > MinAudibleVolume)
				{
					try
					{
                        SoundBuffer soundBuffer = ContentManager.Get<SoundBuffer>(name);
                        Sound sound = new(soundBuffer, num, ToEnginePitch(pitch), pan, isLooped: false, disposeOnStop: true);
						sound.Play(new OpenTK.Vector3(vector.X,vector.Y,vector.Z));
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
