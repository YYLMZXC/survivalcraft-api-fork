using System.IO;
namespace Game.IContentReader
{
	public class JsonObjectReader : IContentReader
	{
		public string Type => "SimpleJson.JsonObject";
		public string[] DefaultSuffix => new string[] { "json" };
		public object Get(ContentInfo[] contents)
		{
			return SimpleJson.SimpleJson.DeserializeObject<SimpleJson.JsonObject>(new StreamReader(contents[0].Duplicate()).ReadToEnd());
		}
	}
}
