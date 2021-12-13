using Engine.Media;

#if desktop
using OpenTK.Audio.OpenAL;
#endif

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Engine.Audio
{
	public class SoundBuffer : IDisposable
	{
#if desktop
		internal int m_buffer;
#else
		internal byte[] m_data;

		internal GCHandle m_gcHandle;
#endif
		public Stream SoundStream = null;

		public int ChannelsCount
		{
			get;
			private set;
		}

		public int SamplingFrequency
		{
			get;
			private set;
		}

		public int SamplesCount
		{
			get;
			private set;
		}

		public int UseCount
		{
			get;
			internal set;
		}
		public string cachePath;
#if desktop
		public SoundBuffer(byte[] data, int startIndex, int itemsCount, int channelsCount, int samplingFrequency)
		{
			Initialize(data, startIndex, itemsCount, channelsCount, samplingFrequency);
			CreateBuffer(data, startIndex, itemsCount, channelsCount, samplingFrequency);
		}

		public SoundBuffer(short[] data, int startIndex, int itemsCount, int channelsCount, int samplingFrequency)
		{
			Initialize(data, startIndex, itemsCount, channelsCount, samplingFrequency);
			CreateBuffer(data, startIndex, itemsCount, channelsCount, samplingFrequency);
		}

		public SoundBuffer(Stream stream, int bytesCount, int channelsCount, int samplingFrequency)
		{
			byte[] array = Initialize(stream, bytesCount, channelsCount, samplingFrequency);
			CreateBuffer(array, 0, array.Length, channelsCount, samplingFrequency);
		}
		public SoundBuffer() { }
		private void InternalDispose()
		{
			if (m_buffer != 0)
			{
				AL.DeleteBuffer(m_buffer);
				Mixer.CheckALError();
				m_buffer = 0;
			}
		}

		private void CreateBuffer<T>(T[] data, int startIndex, int itemsCount, int channelsCount, int samplingFrequency)
		{
			m_buffer = AL.GenBuffer();
			Mixer.CheckALError();
			GCHandle gCHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			try
			{
				int num = Utilities.SizeOf<T>();
				AL.BufferData(m_buffer, (channelsCount == 1) ? ALFormat.Mono16 : ALFormat.Stereo16, gCHandle.AddrOfPinnedObject() + startIndex * num, itemsCount * num, samplingFrequency);
				Mixer.CheckALError();
			}
			finally
			{
				gCHandle.Free();
			}
		}
#else

		public SoundBuffer(byte[] data, int startIndex, int itemsCount, int channelsCount, int samplingFrequency)
		{
			Initialize(data, startIndex, itemsCount, channelsCount, samplingFrequency);
			m_data = new byte[itemsCount];
			Buffer.BlockCopy(data, startIndex, m_data, 0, itemsCount);
		}

		public SoundBuffer(short[] data, int startIndex, int itemsCount, int channelsCount, int samplingFrequency)
		{
			Initialize(data, startIndex, itemsCount, channelsCount, samplingFrequency);
			m_data = new byte[2 * itemsCount];
			Buffer.BlockCopy(data, startIndex, m_data, 0, itemsCount * 2);
		}

		public SoundBuffer(Stream stream, int bytesCount, int channelsCount, int samplingFrequency)
		{
			m_data = Initialize(stream, bytesCount, channelsCount, samplingFrequency);
		}

		public void InternalDispose()
		{
			if (m_gcHandle.IsAllocated)
			{
				m_gcHandle.Free();
			}
		}

		internal GCHandle GetPinnedHandle()
		{
			if (!m_gcHandle.IsAllocated)
			{
				m_gcHandle = GCHandle.Alloc(m_data, GCHandleType.Pinned);
			}
			return m_gcHandle;
		}

#endif

		public void Dispose()
		{
			if (UseCount != 0)
			{
				throw new InvalidOperationException("Cannot dispose SoundBuffer which is in use.");
			}
			InternalDispose();
		}

		public static SoundBuffer Load(SoundData soundData)
		{
			return new SoundBuffer(soundData.Data, 0, soundData.Data.Length, soundData.ChannelsCount, soundData.SamplingFrequency);
		}

		public static SoundBuffer Load(Stream stream, SoundFileFormat format)
		{
			return Load(SoundData.Load(stream, format));
		}

		public static SoundBuffer Load(string fileName, SoundFileFormat format)
		{
			return Load(SoundData.Load(fileName, format));
		}

		public static SoundBuffer Load(Stream stream)
		{
			return Load(SoundData.Load(stream));
		}

		public static SoundBuffer Load(string fileName)
		{
			return Load(SoundData.Load(fileName));
		}

		private void InitializeProperties(int samplesCount, int channelsCount, int samplingFrequency)
		{
			if (samplesCount <= 0)
			{
				throw new InvalidOperationException("Buffer cannot have zero samples.");
			}
			if (channelsCount < 1 || channelsCount > 2)
			{
				throw new ArgumentOutOfRangeException("channelsCount");
			}
			if (samplingFrequency < 8000 || samplingFrequency > 48000)
			{
				throw new ArgumentOutOfRangeException("samplingFrequency");
			}
			ChannelsCount = channelsCount;
			SamplingFrequency = samplingFrequency;
			SamplesCount = samplesCount;
		}

		private void Initialize<T>(T[] data, int startIndex, int itemsCount, int channelsCount, int samplingFrequency)
		{
			int num = Utilities.SizeOf<T>();
			InitializeProperties(itemsCount * num / channelsCount / 2, channelsCount, samplingFrequency);
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			if (startIndex + itemsCount > data.Length)
			{
				throw new ArgumentOutOfRangeException("itemsCount");
			}
		}

		private byte[] Initialize(Stream stream, int bytesCount, int channelsCount, int samplingFrequency)
		{
			byte[] array = new byte[bytesCount];
			if (stream.Read(array, 0, bytesCount) != bytesCount)
			{
				throw new InvalidOperationException("Not enough data in stream.");
			}
			Initialize(array, 0, bytesCount, channelsCount, samplingFrequency);
			return array;
		}
	}
}