using System.IO;

namespace Game.IContentReader
{
    public class JsonArrayReader:IContentReader
    {
        public override string Type => "SimpleJson.JsonArray";
        public override string[] DefaultSuffix => new string[] { "json" };
        public override object Get(ContentInfo[] contents)
        {
            return SimpleJson.SimpleJson.DeserializeObject<SimpleJson.JsonArray>(new StreamReader(contents[0].Duplicate()).ReadToEnd());
        }
    }
}
