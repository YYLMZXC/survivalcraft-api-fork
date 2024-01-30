using System.IO;
namespace Game.IContentReader
{
	public class StringReader : IContentReader
	{
		public string Type => "System.String";
		public string[] DefaultSuffix => new string[] { "txt" };
		public object Get(ContentInfo[] contents)
		{
			return new StreamReader(contents[0].Duplicate()).ReadToEnd();
		}
	}
}
