using Android.Media;
using System;

namespace Engine.Audio
{
    public abstract class BaseSound : IDisposable
	{
		internal AudioTrack m_audioTrack;

		internal int m_stopPosition = -1;

		private float m_volume = 1f;

		private float m_pitch = 1f;

		private float m_pan;

		internal object m_lock = new object();

		internal bool m_isLooped;

		internal bool m_disposeOnStop;

		public SoundState State { get; internal set; }

		public int ChannelsCount { get; internal set; }

		public int SamplingFrequency { get; internal set; }

		public float Volume
		{
			get
			{
				return m_volume;
			}
			set
			{
				value = MathUtils.Saturate(value);
				if (value != m_volume)
				{
					m_volume = value;
					InternalSetVolume(value);
				}
			}
		}

		public float Pitch
		{
			get
			{
				return m_pitch;
			}
			set
			{
				value = MathUtils.Clamp(value, 0.5f, 2f);
				if (value != m_pitch)
				{
					m_pitch = value;
					InternalSetPitch(value);
				}
			}
		}

		public float Pan
		{
			get
			{
				return m_pan;
			}
			set
			{
				if (ChannelsCount == 1)
				{
					value = MathUtils.Clamp(value, -1f, 1f);
					if (value != m_pan)
					{
						m_pan = value;
						InternalSetPan(value);
					}
				}
			}
		}

		public bool IsLooped
		{
			get
			{
				return m_isLooped;
			}
			set
			{
				lock (m_lock)
				{
					if (State == SoundState.Stopped)
					{
						m_isLooped = value;
					}
				}
			}
		}

		public bool DisposeOnStop
		{
			get
			{
				return m_disposeOnStop;
			}
			set
			{
				lock (m_lock)
				{
					if (State == SoundState.Stopped)
					{
						m_disposeOnStop = value;
					}
				}
			}
		}

		internal BaseSound()
		{
			Mixer.AddSound(this);
		}

		internal abstract void InternalPlay();

		internal abstract void InternalPause();

		internal abstract void InternalStop();

		internal virtual void InternalDispose()
		{
			Mixer.RemoveSound(this);
			if (m_audioTrack != null)
			{
				m_audioTrack.Pause();
				m_audioTrack.Release();
				m_audioTrack.Dispose();
				m_audioTrack = null;
				Mixer.m_audioTracksDestroyed++;
			}
		}

		internal void InternalSetVolume(float volume)
		{
			if (m_audioTrack != null)
			{
				CalculateStereoVolumes(volume * Mixer.MasterVolume, Pan, out var left, out var right);
				Mixer.CheckTrackStatus(m_audioTrack.SetStereoVolume(left, right));
			}
		}

		internal void InternalSetPitch(float pitch)
		{
			if (m_audioTrack != null)
			{
				int x = (int)((float)SamplingFrequency * pitch);
				x = MathUtils.Min(x, 2 * AudioTrack.GetNativeOutputSampleRate(Stream.Music));
				Mixer.CheckTrackStatus((TrackStatus)m_audioTrack.SetPlaybackRate(x));
			}
		}

		internal void InternalSetPan(float pan)
		{
			if (m_audioTrack != null)
			{
				CalculateStereoVolumes(Volume, pan, out var left, out var right);
				Mixer.CheckTrackStatus(m_audioTrack.SetStereoVolume(left, right));
			}
		}

		private static void CalculateStereoVolumes(float volume, float pan, out float left, out float right)
		{
			left = volume * MathUtils.Saturate(0f - pan + 1f);
			right = volume * MathUtils.Saturate(pan + 1f);
		}

		public void Play()
		{
			lock (m_lock)
			{
				if (State == SoundState.Stopped || State == SoundState.Paused)
				{
					State = SoundState.Playing;
					InternalPlay();
				}
			}
		}

		public void Pause()
		{
			lock (m_lock)
			{
				if (State == SoundState.Playing)
				{
					State = SoundState.Paused;
					InternalPause();
				}
			}
		}

		public void Stop()
		{
			if (m_disposeOnStop)
			{
				Dispose();
			}
			lock (m_lock)
			{
				if (State == SoundState.Playing || State == SoundState.Paused)
				{
					State = SoundState.Stopped;
					InternalStop();
				}
			}
		}

		public virtual void Dispose()
		{
			if (State != SoundState.Disposed)
			{
				State = SoundState.Disposed;
				InternalDispose();
			}
		}
	}
}