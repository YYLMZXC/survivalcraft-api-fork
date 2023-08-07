using System.IO;
namespace Game.IContentReader
{
	public class JsonObjectReader : IContentReader
	{
		public override string Type => "SimpleJson.JsonObject";
		public override string[] DefaultSuffix => new string[] { "json" };
		public override object Get(ContentInfo[] contents)
		{
			return SimpleJson.SimpleJson.DeserializeObject<SimpleJson.JsonObject>(new StreamReader(contents[0].Duplicate()).ReadToEnd());
		}
	}
}
