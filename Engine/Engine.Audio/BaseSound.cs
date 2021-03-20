using OpenTK.Audio.OpenAL;
using System;

namespace Engine.Audio
{
	public abstract class BaseSound : IDisposable
	{
		public float m_volume = 1f;

		public float m_pitch = 1f;

		public float m_pan;

		public object m_stateSync = new object();

		public bool m_isLooped;

		public bool m_disposeOnStop;

		public int m_source;

		public SoundState State
		{
			get;
			set;
		}

		public int ChannelsCount
		{
			get;
			set;
		}

		public int SamplingFrequency
		{
			get;
			set;
		}

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
					publicSetVolume(value);
					m_volume = value;
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
					publicSetPitch(value);
					m_pitch = value;
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
						publicSetPan(value);
						m_pan = value;
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
				lock (m_stateSync)
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
				lock (m_stateSync)
				{
					if (State == SoundState.Stopped)
					{
						m_disposeOnStop = value;
					}
				}
			}
		}

		public void Play()
		{
			lock (m_stateSync)
			{
				if (State == SoundState.Stopped || State == SoundState.Paused)
				{
					State = SoundState.Playing;
					publicPlay();
				}
			}
		}

		public void Pause()
		{
			lock (m_stateSync)
			{
				if (State == SoundState.Playing)
				{
					State = SoundState.Paused;
					publicPause();
				}
			}
		}

		public void Stop()
		{
			if (m_disposeOnStop)
			{
				Dispose();
			}
			lock (m_stateSync)
			{
				if (State == SoundState.Playing || State == SoundState.Paused)
				{
					State = SoundState.Stopped;
					publicStop();
				}
			}
		}

		public virtual void Dispose()
		{
			if (State != SoundState.Disposed)
			{
				State = SoundState.Disposed;
				publicDispose();
			}
		}

		public BaseSound()
		{
			m_source = AL.GenSource();
			Mixer.CheckALError();
			AL.DistanceModel(ALDistanceModel.None);
			Mixer.CheckALError();
		}

		public void publicSetVolume(float volume)
		{
			if (m_source != 0)
			{
				AL.Source(m_source, ALSourcef.Gain, volume);
				Mixer.CheckALError();
			}
		}

		public void publicSetPitch(float pitch)
		{
			if (m_source != 0)
			{
				AL.Source(m_source, ALSourcef.Pitch, pitch);
				Mixer.CheckALError();
			}
		}

		public void publicSetPan(float pan)
		{
			if (m_source != 0)
			{
				float value = 0f;
				float value2 = -0.1f;
				AL.Source(m_source, ALSource3f.Position, pan, value, value2);
				Mixer.CheckALError();
			}
		}

		public abstract void publicPlay();

		public abstract void publicPause();

		public abstract void publicStop();

		public virtual void publicDispose()
		{
			if (m_source != 0)
			{
				AL.SourceStop(m_source);
				Mixer.CheckALError();
				AL.DeleteSource(m_source);
				Mixer.CheckALError();
				m_source = 0;
			}
		}
	}
}
