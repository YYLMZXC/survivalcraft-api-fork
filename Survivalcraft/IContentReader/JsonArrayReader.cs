using System.IO;

namespace Game.IContentReader
{
	public class JsonArrayReader : IContentReader
	{
		public string Type => "SimpleJson.JsonArray";
		public string[] DefaultSuffix => new string[] { "json" };
		public object Get(ContentInfo[] contents)
		{
			return SimpleJson.SimpleJson.DeserializeObject<SimpleJson.JsonArray>(new StreamReader(contents[0].Duplicate()).ReadToEnd());
		}
	}
}
