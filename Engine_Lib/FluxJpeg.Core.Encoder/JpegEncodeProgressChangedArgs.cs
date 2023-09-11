using System;

namespace FluxJpeg.Core.Encoder
{
	public class JpegEncodeProgressChangedArgs : EventArgs
	{
		public double EncodeProgress;
	}
}
