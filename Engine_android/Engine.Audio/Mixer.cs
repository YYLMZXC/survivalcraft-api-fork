using Android.Media;
using Android.OS;
using System;
using System.Collections.Generic;

namespace Engine.Audio
{
	public static class Mixer
	{
		internal static int m_audioTracksCreated;

		internal static int m_audioTracksDestroyed;

		private static HashSet<BaseSound> m_sounds = new HashSet<BaseSound>();

		private static List<BaseSound> m_soundsToStop = new List<BaseSound>();

		private static List<BaseSound> m_pausedSounds = new List<BaseSound>();

		private static float m_masterVolume = 1f;

		public static bool EnableAudioTrackCaching { get; set; }

		public static float MasterVolume
		{
			get
			{
				return m_masterVolume;
			}
			set
			{
				value = MathUtils.Saturate(value);
				if (value != m_masterVolume)
				{
					m_masterVolume = value;
					InternalSetMasterVolume(value);
				}
			}
		}

		internal static void Initialize()
		{
			Window.Activity.VolumeControlStream = Stream.Music;
			EnableAudioTrackCaching = Build.VERSION.SdkInt < BuildVersionCodes.Kitkat;
		}

		internal static void Dispose()
		{
		}

		internal static void Activate()
		{
			foreach (BaseSound pausedSound in m_pausedSounds)
			{
				pausedSound.Play();
			}
			m_pausedSounds.Clear();
		}

		internal static void Deactivate()
		{
			lock (m_sounds)
			{
				foreach (BaseSound sound in m_sounds)
				{
					if (sound.State == SoundState.Playing)
					{
						sound.Pause();
						m_pausedSounds.Add(sound);
					}
				}
			}
		}

		internal static void BeforeFrame()
		{
			lock (m_sounds)
			{
				foreach (BaseSound sound in m_sounds)
				{
					lock (sound.m_lock)
					{
						AudioTrack audioTrack = sound.m_audioTrack;
						if (audioTrack != null && sound.State == SoundState.Playing && sound.m_stopPosition >= 0 && audioTrack.PlaybackHeadPosition >= sound.m_stopPosition)
						{
							m_soundsToStop.Add(sound);
						}
					}
				}
			}
			foreach (BaseSound item in m_soundsToStop)
			{
				item.Stop();
			}
			m_soundsToStop.Clear();
		}

		internal static void AfterFrame()
		{
		}

		internal static void InternalSetMasterVolume(float volume)
		{
			lock (m_sounds)
			{
				foreach (BaseSound sound in m_sounds)
				{
					sound.InternalSetVolume(sound.Volume);
				}
			}
		}

		internal static void CheckTrackStatus(TrackStatus status)
		{
			if (status != 0)
			{
				throw new InvalidOperationException("AudioTrack error " + status.ToString() + ".");
			}
		}

		internal static void AddSound(BaseSound sound)
		{
			lock (m_sounds)
			{
				m_sounds.Add(sound);
			}
		}

		internal static void RemoveSound(BaseSound sound)
		{
			lock (m_sounds)
			{
				m_sounds.Remove(sound);
			}
		}
	}

}