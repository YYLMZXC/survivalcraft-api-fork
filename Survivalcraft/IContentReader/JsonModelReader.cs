namespace Game.IContentReader
{
	public class JsonModelReader : IContentReader
	{
		public string Type => "Game.JsonModel";
		public string[] DefaultSuffix => new string[] { "json" };
		public object Get(ContentInfo[] contents)
		{
			return Game.JsonModelReader.Load(contents[0].Duplicate());
		}
	}
}
