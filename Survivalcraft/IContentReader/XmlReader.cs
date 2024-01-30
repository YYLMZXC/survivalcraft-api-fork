using System.Xml.Linq;
namespace Game.IContentReader
{
	public class XmlReader : IContentReader
	{
		public string Type => "System.Xml.Linq.XElement";
		public string[] DefaultSuffix => new string[] { "xml", "xdb" };
		public object Get(ContentInfo[] contents)
		{
			return XElement.Load(contents[0].Duplicate());
		}
	}
}
