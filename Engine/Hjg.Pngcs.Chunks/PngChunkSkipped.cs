namespace Hjg.Pngcs.Chunks
{
	internal class PngChunkSkipped : PngChunk
	{
		internal PngChunkSkipped(string id, ImageInfo imgInfo, int clen)
			: base(id, imgInfo)
		{
			base.Length = clen;
		}

		public  override bool AllowsMultiple()
		{
			return true;
		}

		public  override ChunkRaw CreateRawChunk()
		{
			throw new PngjException("Non supported for a skipped chunk");
		}

		public  override void ParseFromRaw(ChunkRaw c)
		{
			throw new PngjException("Non supported for a skipped chunk");
		}

		public  override void CloneDataFromRead(PngChunk other)
		{
			throw new PngjException("Non supported for a skipped chunk");
		}

		public override ChunkOrderingConstraint GetOrderingConstraint()
		{
			return ChunkOrderingConstraint.NONE;
		}
	}
}
