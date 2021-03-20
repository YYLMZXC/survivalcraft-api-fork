using OpenTK.Audio.OpenAL;
using System;

namespace Engine.Audio
{
	public sealed class Sound : BaseSound
	{
		public SoundBuffer m_soundBuffer;

		public SoundBuffer SoundBuffer => m_soundBuffer;

		public override void Dispose()
		{
			base.Dispose();
			if (m_soundBuffer != null)
			{
				int num = --m_soundBuffer.UseCount;
				m_soundBuffer = null;
			}
		}

		public void Initialize(SoundBuffer soundBuffer)
		{
			if (soundBuffer == null)
			{
				throw new ArgumentNullException("soundBuffer");
			}
			m_soundBuffer = soundBuffer;
			int num = ++m_soundBuffer.UseCount;
		}

		public Sound(SoundBuffer soundBuffer, float volume = 1f, float pitch = 1f, float pan = 0f, bool isLooped = false, bool disposeOnStop = false)
		{
			if (soundBuffer == null)
			{
				throw new ArgumentNullException("soundBuffer");
			}
			AL.Source(m_source, ALSourcei.Buffer, soundBuffer.m_buffer);
			Mixer.CheckALError();
			Initialize(soundBuffer);
			base.ChannelsCount = soundBuffer.ChannelsCount;
			base.SamplingFrequency = soundBuffer.SamplingFrequency;
			base.Volume = volume;
			base.Pitch = pitch;
			base.Pan = pan;
			base.IsLooped = isLooped;
			base.DisposeOnStop = disposeOnStop;
			Mixer.m_soundsToStopPoll.Add(this);
		}

		public override void publicPlay()
		{
			AL.Source(m_source, ALSourceb.Looping, m_isLooped);
			AL.SourcePlay(m_source);
			Mixer.CheckALError();
		}

		public override void publicPause()
		{
			AL.SourcePause(m_source);
			Mixer.CheckALError();
		}

		public override void publicStop()
		{
			AL.SourceRewind(m_source);
			Mixer.CheckALError();
		}

		public override void publicDispose()
		{
			base.publicDispose();
			Mixer.m_soundsToStopPoll.Remove(this);
		}
	}
}
