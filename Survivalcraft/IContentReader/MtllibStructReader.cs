namespace Game.IContentReader
{
	public class MtllibStructReader : IContentReader
	{
		public string Type => "Game.MtllibStruct";
		public string[] DefaultSuffix => new string[] { "mtl" };
		public object Get(ContentInfo[] contents)
		{
			return MtllibStruct.Load(contents[0].Duplicate());
		}
	}
}
