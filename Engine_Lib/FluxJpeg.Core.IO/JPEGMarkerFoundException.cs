using System;

namespace FluxJpeg.Core.IO
{
	public class JPEGMarkerFoundException : Exception
	{
		public byte Marker;

		public JPEGMarkerFoundException(byte marker)
		{
			Marker = marker;
		}
	}
}
