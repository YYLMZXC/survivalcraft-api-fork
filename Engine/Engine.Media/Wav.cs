using Engine.Serialization;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Engine.Media
{
	public static class Wav
	{
		public class WavStreamingSource : StreamingSource
		{
			private Stream m_stream;

			private bool m_leaveOpen;

			private int m_channelsCount;

			private int m_samplingFrequency;

			private long m_bytesCount;

			private long m_position;

			public MemoryStream BaseStream = new MemoryStream();

			public override int ChannelsCount => m_channelsCount;

			public override int SamplingFrequency => m_samplingFrequency;

			public override long BytesCount => m_bytesCount;

			public override long Position
			{
				get
				{
					return m_position;
				}
				set
				{
					if (m_stream.CanSeek)
					{
						long num = value * ChannelsCount * 2;
						if (num < 0 || num > BytesCount)
						{
							throw new ArgumentOutOfRangeException();
						}
						m_stream.Position = Utilities.SizeOf<WavHeader>() + num;
						m_position = value;
						return;
					}
					throw new NotSupportedException("Underlying stream cannot be seeked.");
				}
			}

			public WavStreamingSource(Stream stream, bool leaveOpen = false)
			{
				stream.CopyTo(BaseStream);
				m_stream = BaseStream;
				BaseStream.Position = 0L;
				m_leaveOpen = leaveOpen;
				ReadHeaders(BaseStream, out FmtHeader fmtHeader, out DataHeader dataHeader, out long dataStart);
				m_channelsCount = fmtHeader.ChannelsCount;
				m_samplingFrequency = fmtHeader.SamplingFrequency;
				m_bytesCount = dataHeader.DataSize;
				BaseStream.Position = dataStart;
			}

			public override void Dispose()
			{
				if (!m_leaveOpen)
				{
					m_stream.Dispose();
				}
				m_stream = null;
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				if (count % (2 * ChannelsCount) != 0)
				{
					throw new InvalidOperationException("Cannot read partial samples.");
				}
				count = (int)MathUtils.Min(count, m_bytesCount - m_position * 2 * ChannelsCount);
				int num = m_stream.Read(buffer, offset, count);
				m_position += num / 2 / ChannelsCount;
				return num;
			}

			public override StreamingSource Duplicate()
			{
				MemoryStream memoryStream = new MemoryStream();
				m_stream.Position = 0L;
				m_stream.CopyTo(memoryStream);
				memoryStream.Position = 0L;
				return new WavStreamingSource(memoryStream);
				throw new InvalidOperationException("Underlying stream does not support duplication.");
			}
		}

		public struct WavInfo
		{
			public int ChannelsCount;

			public int SamplingFrequency;

			public int BytesCount;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct WavHeader
		{
			public int Riff;

			public int FileSize;

			public int Wave;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct FmtHeader
		{
			public int Fmt;

			public int FormatSize;

			public short Type;

			public short ChannelsCount;

			public int SamplingFrequency;

			public int BytesPerSecond;

			public short BytesPerSample;

			public short BitsPerChannel;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct DataHeader
		{
			public int Data;

			public int DataSize;
		}

		public static bool IsWavStream(Stream stream)
		{
			var binaryReader = new BinaryReader(stream);
			if (stream.Length - stream.Position >= Utilities.SizeOf<WavHeader>())
			{
				int num = binaryReader.ReadInt32();
				int num2 = binaryReader.ReadInt32();
				int num3 = binaryReader.ReadInt32();
				stream.Position -= 12L;
				if (num == MakeFourCC("RIFF") && num2 != 0 && num3 == MakeFourCC("WAVE"))
				{
					return true;
				}
			}
			return false;
		}

		public static WavInfo GetInfo(Stream stream)
		{
			ReadHeaders(stream, out FmtHeader fmtHeader, out DataHeader dataHeader, out long _);
			var result = default(WavInfo);
			result.ChannelsCount = fmtHeader.ChannelsCount;
			result.SamplingFrequency = fmtHeader.SamplingFrequency;
			result.BytesCount = dataHeader.DataSize;
			return result;
		}

		public static StreamingSource Stream(Stream stream)
		{
			return new WavStreamingSource(stream);
		}

		public static SoundData Load(Stream stream)
		{
			ReadHeaders(stream, out FmtHeader fmtHeader, out DataHeader dataHeader, out long dataStart);
			stream.Position = dataStart;
			var soundData = new SoundData(fmtHeader.ChannelsCount, fmtHeader.SamplingFrequency, dataHeader.DataSize);
			byte[] array = new byte[dataHeader.DataSize];
			if (stream.Read(array, 0, array.Length) != array.Length)
			{
				throw new InvalidOperationException("Truncated WAV data.");
			}
			Buffer.BlockCopy(array, 0, soundData.Data, 0, array.Length);
			return soundData;
		}

		public static void Save(SoundData soundData, Stream stream)
		{
			if (soundData == null)
			{
				throw new ArgumentNullException(nameof(soundData));
			}
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}
			var engineBinaryWriter = new EngineBinaryWriter(stream);
			var structure = default(WavHeader);
			structure.Riff = MakeFourCC("RIFF");
			structure.FileSize = Utilities.SizeOf<WavHeader>() + Utilities.SizeOf<FmtHeader>() + Utilities.SizeOf<DataHeader>() + soundData.Data.Length;
			structure.Wave = MakeFourCC("WAVE");
			engineBinaryWriter.WriteStruct(structure);
			var structure2 = default(FmtHeader);
			structure2.Fmt = MakeFourCC("fmt ");
			structure2.FormatSize = 16;
			structure2.Type = 1;
			structure2.ChannelsCount = (short)soundData.ChannelsCount;
			structure2.SamplingFrequency = soundData.SamplingFrequency;
			structure2.BytesPerSecond = soundData.ChannelsCount * 2 * soundData.SamplingFrequency;
			structure2.BytesPerSample = (short)(soundData.ChannelsCount * 2);
			structure2.BitsPerChannel = 16;
			engineBinaryWriter.WriteStruct(structure2);
			var structure3 = default(DataHeader);
			structure3.Data = MakeFourCC("data");
			structure3.DataSize = soundData.Data.Length * 2;
			engineBinaryWriter.WriteStruct(structure3);
			byte[] array = new byte[soundData.Data.Length * 2];
			Buffer.BlockCopy(soundData.Data, 0, array, 0, array.Length);
			stream.Write(array, 0, array.Length);
		}

		private static void ReadHeaders(Stream stream, out FmtHeader fmtHeader, out DataHeader dataHeader, out long dataStart)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}
			if (!BitConverter.IsLittleEndian)
			{
				throw new InvalidOperationException("Unsupported system endianness.");
			}
			if (!IsWavStream(stream))
			{
				throw new InvalidOperationException("Invalid WAV header.");
			}
			var engineBinaryReader = new EngineBinaryReader(stream);
			fmtHeader = default(FmtHeader);
			dataHeader = default(DataHeader);
			dataStart = 0L;
			stream.Position += 12L;
			bool flag = false;
			bool flag2 = false;
			while (!flag || !flag2)
			{
				int num = engineBinaryReader.ReadInt32();
				if (num == MakeFourCC("fmt "))
				{
					stream.Position -= 4L;
					fmtHeader = engineBinaryReader.ReadStruct<FmtHeader>();
					flag = true;
				}
				else if (num == MakeFourCC("data"))
				{
					stream.Position -= 4L;
					dataHeader = engineBinaryReader.ReadStruct<DataHeader>();
					dataStart = stream.Position;
					flag2 = true;
				}
				else
				{
					int num2 = engineBinaryReader.ReadInt32();
					stream.Position += num2;
				}
			}
			if (fmtHeader.Type != 1 || fmtHeader.ChannelsCount < 1 || fmtHeader.ChannelsCount > 2 || fmtHeader.SamplingFrequency < 8000 || fmtHeader.SamplingFrequency > 48000 || fmtHeader.BitsPerChannel != 16)
			{
				throw new InvalidOperationException("Unsupported WAV format.");
			}
		}

		private static int MakeFourCC(string text)
		{
			return (int)(((uint)text[3] << 24) | ((uint)text[2] << 16) | ((uint)text[1] << 8) | text[0]);
		}
	}
}
