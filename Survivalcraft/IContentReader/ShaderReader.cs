using Engine.Graphics;
using System.IO;

namespace Game.IContentReader
{
	public class ShaderReader : IContentReader
	{
		public override string Type => "Engine.Graphics.Shader";
		public override string[] DefaultSuffix => new string[] { "vsh", "psh" };
		public override object Get(ContentInfo[] contents)
		{
			return new Shader(new StreamReader(contents[0].Duplicate()).ReadToEnd(), new StreamReader(contents[1].Duplicate()).ReadToEnd(), new ShaderMacro[] { new ShaderMacro("empty") });
		}
	}
}
