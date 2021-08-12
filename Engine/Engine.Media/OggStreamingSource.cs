using NVorbis;
using System;
using System.IO;


namespace Engine.Media
{
	public class OggStreamingSource : Ogg.OggStreamingSource
	{
		public OggStreamingSource(Stream stream, bool leaveOpen = false) : base(stream, leaveOpen) { }
	}
}
