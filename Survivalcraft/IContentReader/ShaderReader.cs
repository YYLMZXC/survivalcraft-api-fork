using Engine.Graphics;
using System.IO;

namespace Game.IContentReader
{
	public class ShaderReader : IContentReader
	{
		public string Type => "Engine.Graphics.Shader";
		public string[] DefaultSuffix => new string[] { "vsh", "psh" };
		public object Get(ContentInfo[] contents)
		{
			return new Shader(new StreamReader(contents[0].Duplicate()).ReadToEnd(), new StreamReader(contents[1].Duplicate()).ReadToEnd(), new ShaderMacro[] { new("empty") });
		}
	}
}
