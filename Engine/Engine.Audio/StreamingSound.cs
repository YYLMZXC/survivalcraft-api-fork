using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Engine.Media;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace Engine.Audio
{
	public sealed class StreamingSound : BaseSound
	{
		private enum Command
		{
			Play,
			Pause,
			Stop,
			Exit
		}

		private Task m_task;

		private BlockingCollection<Command> m_queue = new BlockingCollection<Command>(100);

		private float m_bufferDuration;

		public StreamingSource StreamingSource { get; private set; }

		private int ReadStreamingSource(byte[] buffer, int count)
		{
			int num = 0;
			if (StreamingSource.BytesCount > 0)
			{
				while (count > 0)
				{
					int num2 = StreamingSource.Read(buffer, num, count);
					if (num2 > 0)
					{
						num += num2;
						count -= num2;
						continue;
					}
					if (!m_isLooped)
					{
						break;
					}
					StreamingSource.Position = 0L;
				}
			}
			return num;
		}

		private void VerifyStreamingSource(StreamingSource streamingSource)
		{
			if (streamingSource == null)
			{
				throw new ArgumentNullException("streamingSource");
			}
			if (streamingSource.ChannelsCount < 1 || streamingSource.ChannelsCount > 2)
			{
				throw new InvalidOperationException("Unsupported channels count.");
			}
			if (streamingSource.SamplingFrequency < 8000 || streamingSource.SamplingFrequency > 48000)
			{
				throw new InvalidOperationException("Unsupported frequency.");
			}
		}

		public StreamingSound(StreamingSource streamingSource, float volume = 1f, float pitch = 1f, float pan = 0f, bool isLooped = false, bool disposeOnStop = false, float bufferDuration = 0.3f)
		{
			VerifyStreamingSource(streamingSource);
			WaveFormat sourceFormat = new WaveFormat(streamingSource.SamplingFrequency, 16, streamingSource.ChannelsCount);
			m_sourceVoice = new SourceVoice(Mixer.m_xAudio2, sourceFormat, VoiceFlags.None, 2f, enableCallbackEvents: false);
			StreamingSource = streamingSource;
			base.ChannelsCount = streamingSource.ChannelsCount;
			base.SamplingFrequency = streamingSource.SamplingFrequency;
			base.Volume = volume;
			base.Pitch = pitch;
			base.Pan = pan;
			base.IsLooped = isLooped;
			base.DisposeOnStop = disposeOnStop;
			m_bufferDuration = MathUtils.Clamp(bufferDuration, 0.01f, 10f);
			m_task = Task.Run(delegate
			{
				try
				{
					StreamingThreadFunction();
				}
				catch (Exception message)
				{
					Log.Error(message);
				}
			});
		}

		internal override void InternalPlay()
		{
			m_queue.Add(Command.Play);
		}

		internal override void InternalPause()
		{
			m_queue.Add(Command.Pause);
		}

		internal override void InternalStop()
		{
			m_queue.Add(Command.Stop);
		}

		internal override void InternalDispose()
		{
			if (m_task != null)
			{
				m_queue.Add(Command.Exit);
				m_task.Wait();
				m_task = null;
			}
			if (m_sourceVoice != null)
			{
				m_sourceVoice.Dispose();
				m_sourceVoice = null;
			}
			if (StreamingSource != null)
			{
				StreamingSource.Dispose();
				StreamingSource = null;
			}
			m_queue.Dispose();
			base.InternalDispose();
		}

		private void StreamingThreadFunction()
		{
			GCHandle[] array = new GCHandle[3];
			try
			{
				int num = 2 * base.ChannelsCount * (int)((float)base.SamplingFrequency * m_bufferDuration / (float)array.Length);
				int num2 = 0;
				byte[][] array2 = new byte[array.Length][];
				for (int i = 0; i < array.Length; i++)
				{
					array2[i] = new byte[num];
					array[i] = GCHandle.Alloc(array2[i], GCHandleType.Pinned);
				}
				bool flag = false;
				bool flag2 = false;
				int num3 = MathUtils.Clamp((int)(0.5f * m_bufferDuration / (float)array.Length * 1000f), 1, 100);
				while (true)
				{
					if (m_queue.TryTake(out var item, flag ? num3 : 100))
					{
						switch (item)
						{
						case Command.Play:
							m_sourceVoice.Start();
							flag = true;
							flag2 = false;
							break;
						case Command.Pause:
							m_sourceVoice.Stop();
							flag = false;
							break;
						case Command.Stop:
							m_sourceVoice.Stop();
							m_sourceVoice.FlushSourceBuffers();
							StreamingSource.Position = 0L;
							num2 = 0;
							flag = false;
							break;
						case Command.Exit:
							m_sourceVoice.Stop();
							return;
						}
					}
					while (flag && m_sourceVoice.State.BuffersQueued < array2.Length)
					{
						byte[] array3 = array2[num2];
						GCHandle gCHandle = array[num2];
						num2 = (num2 + 1) % array2.Length;
						int num4 = ReadStreamingSource(array3, array3.Length);
						if (num4 > 0)
						{
							AudioBuffer audioBuffer = new AudioBuffer(new DataPointer(gCHandle.AddrOfPinnedObject(), array3.Length));
							audioBuffer.PlayLength = num4 / m_sourceVoice.VoiceDetails.InputChannelCount / 2;
							audioBuffer.Flags = BufferFlags.None;
							m_sourceVoice.SubmitSourceBuffer(audioBuffer, null);
						}
						if (num4 < array3.Length)
						{
							m_sourceVoice.Discontinuity();
							flag2 = true;
							break;
						}
					}
					if (flag2 && m_sourceVoice.State.BuffersQueued == 0)
					{
						flag2 = false;
						Dispatcher.Dispatch(base.Stop);
					}
				}
			}
			finally
			{
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j].IsAllocated)
					{
						array[j].Free();
					}
				}
			}
		}
	}
}
