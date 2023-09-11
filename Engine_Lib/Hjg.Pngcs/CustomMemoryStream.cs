using System.IO;

namespace Hjg.Pngcs
{
	public class CustomMemoryStream : MemoryStream
	{
		public new virtual void Close()
		{
		}
	}
}
