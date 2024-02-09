using NVorbis.Ogg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NVorbis
{
	internal class VorbisReader : IDisposable
	{
		private int _streamIdx;

		private IContainerReader _containerReader;

		private List<VorbisStreamDecoder> _decoders;

		private List<int> _serials;

		private VorbisStreamDecoder ActiveDecoder
		{
			get
			{
				if (_decoders == null)
				{
					throw new ObjectDisposedException("VorbisReader");
				}
				return _decoders[_streamIdx];
			}
		}

		public int Channels => ActiveDecoder._channels;

		public int SampleRate => ActiveDecoder._sampleRate;

		public int UpperBitrate => ActiveDecoder._upperBitrate;

		public int NominalBitrate => ActiveDecoder._nominalBitrate;

		public int LowerBitrate => ActiveDecoder._lowerBitrate;

		public string Vendor => ActiveDecoder._vendor;

		public string[] Comments => ActiveDecoder._comments;

		public bool IsParameterChange => ActiveDecoder.IsParameterChange;

		public long ContainerOverheadBits => ActiveDecoder.ContainerBits;

		public bool ClipSamples
		{
			get;
			set;
		}

		public IVorbisStreamStatus[] Stats => _decoders.Select((VorbisStreamDecoder d) => d).Cast<IVorbisStreamStatus>().ToArray();

		public int StreamIndex => _streamIdx;

		public int StreamCount => _decoders.Count;

		public TimeSpan DecodedTime
		{
			get
			{
				return TimeSpan.FromSeconds((double)ActiveDecoder.CurrentPosition / (double)SampleRate);
			}
			set
			{
				ActiveDecoder.SeekTo((long)(value.TotalSeconds * (double)SampleRate));
			}
		}

		public long DecodedPosition
		{
			get
			{
				return ActiveDecoder.CurrentPosition;
			}
			set
			{
				ActiveDecoder.SeekTo(value);
			}
		}

		public TimeSpan TotalTime
		{
			get
			{
				VorbisStreamDecoder activeDecoder = ActiveDecoder;
				if (activeDecoder.CanSeek)
				{
					return TimeSpan.FromSeconds((double)activeDecoder.GetLastGranulePos() / (double)activeDecoder._sampleRate);
				}
				return TimeSpan.MaxValue;
			}
		}

		public long TotalSamples
		{
			get
			{
				VorbisStreamDecoder activeDecoder = ActiveDecoder;
				if (activeDecoder.CanSeek)
				{
					return activeDecoder.GetLastGranulePos();
				}
				return long.MaxValue;
			}
		}

		private VorbisReader()
		{
			ClipSamples = true;
			_decoders = [];
			_serials = [];
		}

		public VorbisReader(Stream stream, bool closeStreamOnDispose)
			: this()
		{
			var bufferedReadStream = new BufferedReadStream(stream)
			{
				CloseBaseStream = closeStreamOnDispose
			};
			var containerReader = new ContainerReader(bufferedReadStream, closeStreamOnDispose);
			if (!LoadContainer(containerReader))
			{
				bufferedReadStream.Dispose();
				throw new InvalidDataException("Could not determine container type!");
			}
			_containerReader = containerReader;
			if (_decoders.Count == 0)
			{
				throw new InvalidDataException("No Vorbis data found!");
			}
		}

		public VorbisReader(IContainerReader containerReader)
			: this()
		{
			if (!LoadContainer(containerReader))
			{
				throw new InvalidDataException("Container did not initialize!");
			}
			_containerReader = containerReader;
			if (_decoders.Count == 0)
			{
				throw new InvalidDataException("No Vorbis data found!");
			}
		}

		public VorbisReader(IPacketProvider packetProvider)
			: this()
		{
			var newStreamEventArgs = new NewStreamEventArgs(packetProvider);
			NewStream(this, newStreamEventArgs);
			if (newStreamEventArgs.IgnoreStream)
			{
				throw new InvalidDataException("No Vorbis data found!");
			}
		}

		private bool LoadContainer(IContainerReader containerReader)
		{
			containerReader.NewStream += NewStream;
			if (!containerReader.Init())
			{
				containerReader.NewStream -= NewStream;
				return false;
			}
			return true;
		}

		private void NewStream(object sender, NewStreamEventArgs ea)
		{
			IPacketProvider packetProvider = ea.PacketProvider;
			var vorbisStreamDecoder = new VorbisStreamDecoder(packetProvider);
			if (vorbisStreamDecoder.TryInit())
			{
				_decoders.Add(vorbisStreamDecoder);
				_serials.Add(packetProvider.StreamSerial);
			}
			else
			{
				ea.IgnoreStream = true;
			}
		}

		public void Dispose()
		{
			if (_decoders != null)
			{
				foreach (VorbisStreamDecoder decoder in _decoders)
				{
					decoder.Dispose();
				}
				_decoders.Clear();
				_decoders = null;
			}
			if (_containerReader != null)
			{
				_containerReader.NewStream -= NewStream;
				_containerReader.Dispose();
				_containerReader = null;
			}
		}

		public int ReadSamples(float[] buffer, int offset, int count)
		{
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(offset));
			}
			if (count < 0 || offset + count > buffer.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}
			count = ActiveDecoder.ReadSamples(buffer, offset, count);
			if (ClipSamples)
			{
				VorbisStreamDecoder vorbisStreamDecoder = _decoders[_streamIdx];
				int num = 0;
				while (num < count)
				{
					buffer[offset] = Utils.ClipValue(buffer[offset], ref vorbisStreamDecoder._clipped);
					num++;
					offset++;
				}
			}
			return count;
		}

		public void ClearParameterChange()
		{
			ActiveDecoder.IsParameterChange = false;
		}

		public bool FindNextStream()
		{
			if (_containerReader == null)
			{
				return false;
			}
			return _containerReader.FindNextStream();
		}

		public bool SwitchStreams(int index)
		{
			if (index < 0 || index >= StreamCount)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}
			if (_decoders == null)
			{
				throw new ObjectDisposedException("VorbisReader");
			}
			if (_streamIdx == index)
			{
				return false;
			}
			VorbisStreamDecoder vorbisStreamDecoder = _decoders[_streamIdx];
			_streamIdx = index;
			VorbisStreamDecoder vorbisStreamDecoder2 = _decoders[_streamIdx];
			if (vorbisStreamDecoder._channels == vorbisStreamDecoder2._channels)
			{
				return vorbisStreamDecoder._sampleRate != vorbisStreamDecoder2._sampleRate;
			}
			return true;
		}
	}
}
