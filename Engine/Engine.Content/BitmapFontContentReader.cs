using Engine.Graphics;
using Engine.Media;
using Engine.Serialization;
using System.IO;

namespace Engine.Content
{
	[ContentReader("Engine.Media.BitmapFont")]
	public class BitmapFontContentReader : IContentReader
	{
		public object Read(ContentStream stream, object existingObject)
		{
			if (existingObject == null)
			{
				return ReadBitmapFont(stream);
			}
			InitializeBitmapFont(stream, (BitmapFont)existingObject);
			return existingObject;
		}

		public static BitmapFont ReadBitmapFont(Stream stream)
		{
			var bitmapFont = new BitmapFont();
			InitializeBitmapFont(stream, bitmapFont);
			return bitmapFont;
		}

		internal static void InitializeBitmapFont(Stream stream, BitmapFont bitmapFont)
		{
			var engineBinaryReader = new EngineBinaryReader(stream);
			int num = engineBinaryReader.ReadInt32();
			var array = new BitmapFont.Glyph[num];
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			stringBuilder.AppendLine(num.ToString());
			for (int i = 0; i < num; i++)
			{
				char code = engineBinaryReader.ReadChar();
				Vector2 texCoord = engineBinaryReader.ReadVector2();
				Vector2 texCoord2 = engineBinaryReader.ReadVector2();
				Vector2 offset = engineBinaryReader.ReadVector2();
				float width = engineBinaryReader.ReadSingle();
				stringBuilder.AppendLine($"{code} {texCoord.X} {texCoord.Y} {texCoord2.X} {texCoord2.Y} {offset.X} {offset.Y} {width}");
				array[i] = new BitmapFont.Glyph(code, texCoord, texCoord2, offset, width);
			}
			float glyphHeight = engineBinaryReader.ReadSingle();
			Vector2 spacing = engineBinaryReader.ReadVector2();
			float scale = engineBinaryReader.ReadSingle();
			char fallbackCode = engineBinaryReader.ReadChar();
			stringBuilder.AppendLine(glyphHeight.ToString());
			stringBuilder.AppendLine($"{spacing.X} {spacing.Y}");
			stringBuilder.AppendLine(scale.ToString());
			stringBuilder.AppendLine(fallbackCode.ToString());
			string s = stringBuilder.ToString();
			Texture2D texture = TextureContentReader.ReadTexture(stream);
			bitmapFont.Initialize(texture, null, array, fallbackCode, glyphHeight, spacing, scale);
		}
	}
}
