#define s

using Android.Media;
using System;
using Engine.Media;
namespace Engine.Audio
{
    public sealed class Sound : BaseSound
	{
		public bool m_audioTrackCreateAttempted;

#if s
		public StreamingSound m_streamingSound;

		public Sound(StreamingSource streamingSource, float volume = 1f, float pitch = 1f, float pan = 0f, bool isLooped = false, bool disposeOnStop = false)
		{
			m_streamingSound = new StreamingSound(streamingSource, volume, pitch, pan, isLooped, disposeOnStop);
			Volume = volume;
			Pitch = pitch;
			Pan = pan;
			IsLooped = isLooped;
			DisposeOnStop = disposeOnStop;
		}
#else
		public SoundBuffer m_soundBuffer;

		public SoundBuffer SoundBuffer => m_soundBuffer;

		public Sound(SoundBuffer soundBuffer, float volume = 1f, float pitch = 1f, float pan = 0f, bool isLooped = false, bool disposeOnStop = false)
		{
			if (soundBuffer == null)
			{
				throw new ArgumentNullException("soundBuffer");
			}
			Initialize(soundBuffer);
			ChannelsCount = soundBuffer.ChannelsCount;
			SamplingFrequency = soundBuffer.SamplingFrequency;
			Volume = volume;
			Pitch = pitch;
			Pan = pan;
			IsLooped = isLooped;
			DisposeOnStop = disposeOnStop;
		}
#endif

		internal override void InternalPlay()
		{
#if s
			if (m_streamingSound != null) {
				m_streamingSound.Play();
			}

#else
			if (m_audioTrack == null)
			{
				if (!m_audioTrackCreateAttempted)
				{
					m_audioTrackCreateAttempted = true;
					AudioManager audioManager = (AudioManager)EngineActivity.m_activity.GetSystemService(Android.Content.Context.AudioService);
					m_audioTrack = new AudioTrack(
						new AudioAttributes.Builder().SetUsage(AudioUsageKind.Game).SetContentType(AudioContentType.Music).Build(),
						new AudioFormat.Builder().SetEncoding(Encoding.Pcm16bit).SetSampleRate(SoundBuffer.SamplingFrequency).SetChannelMask(ChannelOut.Default).Build(),
						2 * SoundBuffer.ChannelsCount * (int)(SoundBuffer.SamplingFrequency * 0.3f), AudioTrackMode.Stream, audioManager.GenerateAudioSessionId());
					if (m_audioTrack != null)
					{
						m_stopPosition = (m_isLooped ? (-1) : (SoundBuffer.SamplesCount - 1));
						InternalSetVolume(base.Volume);
						InternalSetPitch(base.Pitch);
						InternalSetPan(base.Pan);
						m_audioTrack.Play();
					}
					else if (!m_isLooped)
					{
						Stop();
					}
				}
			}
			else
			{
				m_audioTrack.SetPlaybackHeadPosition(m_audioTrack.PlaybackHeadPosition);
				m_audioTrack.Play();
			}
#endif
		}

		internal override void InternalPause()
		{
#if s
			if (m_streamingSound != null) m_streamingSound.Pause();
#else
			if (m_audioTrack != null)
			{
				m_audioTrack.Pause();
			}

#endif
		}

		internal override void InternalStop()
		{
#if s
			if (m_streamingSound != null) {
				m_streamingSound.Stop();
			}
#else
if (m_audioTrack != null)
			{
				AudioTrackCache.ReturnAudioTrack(m_audioTrack);
				m_audioTrack = null;
			}
			m_audioTrackCreateAttempted = false;

#endif
		}

		internal override void InternalDispose()
		{
#if s
			if (m_streamingSound != null) m_streamingSound.Dispose();
#else
			if (m_audioTrack != null)
			{
				AudioTrackCache.ReturnAudioTrack(m_audioTrack);
				m_audioTrack = null;
			}
#endif
			base.InternalDispose();
		}

		public override void Dispose()
		{
			base.Dispose();
#if s
			m_streamingSound = null;
#else
			if (m_soundBuffer != null)
			{
				int num = --m_soundBuffer.UseCount;
				m_soundBuffer = null;
			}
#endif
		}

		internal void Initialize(SoundBuffer soundBuffer)
		{
#if s

#else
			if (soundBuffer == null)
			{
				throw new ArgumentNullException("soundBuffer");
			}
			m_soundBuffer = soundBuffer;
			int num = ++m_soundBuffer.UseCount;
#endif
		}
        internal override void InternalSetPan(float pan)
        {
#if s
			m_streamingSound.Pan = pan;
#endif
		}
		internal override void InternalSetPitch(float pitch)
        {
#if s
			m_streamingSound.Pitch = pitch;
#endif
		}
		internal override void InternalSetVolume(float volume)
        {
#if s
			m_streamingSound.Volume = volume;
#endif
		}

	}
}