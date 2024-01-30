namespace Game.IContentReader
{
	public class ObjModelReader : IContentReader
	{
		public string Type => "Game.ObjModel";
		public string[] DefaultSuffix => new string[] { "obj" };
		public object Get(ContentInfo[] contents)
		{
			return Game.ObjModelReader.Load(contents[0].Duplicate());
		}
	}
}
