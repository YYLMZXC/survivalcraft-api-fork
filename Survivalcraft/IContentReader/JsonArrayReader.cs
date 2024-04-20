using System.IO;
using System.Text.Json;

namespace Game.IContentReader
{
	public class JsonArrayReader : IContentReader
	{
		public override string Type => "JsonArray";
		public override string[] DefaultSuffix => new string[] { "json" };
		public override object Get(ContentInfo[] contents)
		{
			JsonElement element = JsonDocument.Parse(new StreamReader(contents[0].Duplicate()).ReadToEnd()).RootElement;
            return element.ValueKind == JsonValueKind.Array ? element : throw new InvalidDataException(contents[0].Filename + "is not Json array");
		}
	}
}
