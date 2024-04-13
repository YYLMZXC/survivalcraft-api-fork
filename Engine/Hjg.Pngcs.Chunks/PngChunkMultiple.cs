namespace Hjg.Pngcs.Chunks
{
	internal abstract class PngChunkMultiple : PngChunk
	{
		internal PngChunkMultiple(string id, ImageInfo imgInfo)
			: base(id, imgInfo)
		{
		}

		public  override bool AllowsMultiple()
		{
			return true;
		}
	}
}
