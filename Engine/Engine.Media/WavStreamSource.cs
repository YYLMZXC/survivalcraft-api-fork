using Engine.Serialization;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Engine.Media
{
	public class WavStreamingSource : Wav.WavStreamingSource
	{
		public WavStreamingSource(Stream stream, bool leaveOpen = false) : base(stream, leaveOpen) { }
	}
}
