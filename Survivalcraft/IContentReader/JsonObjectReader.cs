using System.IO;
using System.Text.Json;
namespace Game.IContentReader
{
	public class JsonObjectReader : IContentReader
	{
		public override string Type => "JsonObject";
		public override string[] DefaultSuffix => new string[] { "json" };
		public override object Get(ContentInfo[] contents)
        {
            JsonElement element = JsonDocument.Parse(new StreamReader(contents[0].Duplicate()).ReadToEnd()).RootElement;
            return element.ValueKind == JsonValueKind.Object ? element : throw new InvalidDataException(contents[0].Filename + "is not Json object");
        }
	}
}
