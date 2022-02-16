using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace Engine.Audio
{
	public sealed class Sound : BaseSound
	{
		private SoundBuffer m_soundBuffer;

		private bool m_bufferSubmitted;

		private GCHandle m_bufferHandle;

		public SoundBuffer SoundBuffer => m_soundBuffer;

		public override void Dispose()
		{
			base.Dispose();
			if (m_soundBuffer != null)
			{
				m_soundBuffer.UseCount--;
				m_soundBuffer = null;
			}
		}

		internal void Initialize(SoundBuffer soundBuffer)
		{
			if (soundBuffer == null)
			{
				throw new ArgumentNullException("soundBuffer");
			}
			m_soundBuffer = soundBuffer;
			m_soundBuffer.UseCount++;
		}

		public Sound(SoundBuffer soundBuffer, float volume = 1f, float pitch = 1f, float pan = 0f, bool isLooped = false, bool disposeOnStop = false)
		{
			Initialize(soundBuffer);
			WaveFormat sourceFormat = new WaveFormat(soundBuffer.SamplingFrequency, 16, soundBuffer.ChannelsCount);
			m_sourceVoice = new SourceVoice(Mixer.m_xAudio2, sourceFormat, VoiceFlags.None, 2f, enableCallbackEvents: true);
			m_sourceVoice.StreamEnd += delegate
			{
				Dispatcher.Dispatch(base.Stop);
			};
			base.ChannelsCount = soundBuffer.ChannelsCount;
			base.SamplingFrequency = soundBuffer.SamplingFrequency;
			base.Volume = volume;
			base.Pitch = pitch;
			base.Pan = pan;
			base.IsLooped = isLooped;
			base.DisposeOnStop = disposeOnStop;
		}

		internal override void InternalPlay()
		{
			if (!m_bufferSubmitted)
			{
				AudioBuffer audioBuffer = new AudioBuffer(new DataPointer(GetBufferPointer(), SoundBuffer.m_data.Length));
				if (m_isLooped)
				{
					audioBuffer.LoopBegin = 0;
					audioBuffer.LoopLength = 0;
					audioBuffer.LoopCount = 255;
				}
				audioBuffer.Flags = BufferFlags.EndOfStream;
				m_sourceVoice.SubmitSourceBuffer(audioBuffer, null);
				m_bufferSubmitted = true;
			}
			m_sourceVoice.Start();
		}

		internal override void InternalPause()
		{
			m_sourceVoice.Stop();
		}

		internal override void InternalStop()
		{
			m_sourceVoice.Stop();
			m_sourceVoice.FlushSourceBuffers();
			m_bufferSubmitted = false;
			FreeBufferPointer();
		}

		internal override void InternalDispose()
		{
			base.InternalDispose();
			FreeBufferPointer();
		}

		private IntPtr GetBufferPointer()
		{
			if (!m_bufferHandle.IsAllocated)
			{
				m_bufferHandle = GCHandle.Alloc(SoundBuffer.m_data, GCHandleType.Pinned);
			}
			return m_bufferHandle.AddrOfPinnedObject();
		}

		private void FreeBufferPointer()
		{
			if (m_bufferHandle.IsAllocated)
			{
				m_bufferHandle.Free();
			}
		}
	}
}
