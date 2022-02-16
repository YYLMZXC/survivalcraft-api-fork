using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Engine.Content;
using Engine.Graphics;

namespace Engine.Media
{
	public class BitmapFont : IDisposable
	{
		public class KerningSettings
		{
			public int Limit = 5;

			public int Tolerance = 1;

			public int BulkingRadius = 1;

			public float BulkingGradient = 1f;
		}

		public class Glyph
		{
			public readonly char Code;

			public readonly bool IsBlank;

			public readonly Vector2 TexCoord1;

			public readonly Vector2 TexCoord2;

			public readonly Vector2 Offset;

			public readonly float Width;

			public Glyph(char code, Vector2 texCoord1, Vector2 texCoord2, Vector2 offset, float width)
			{
				Code = code;
				IsBlank = texCoord1 == texCoord2;
				TexCoord1 = texCoord1;
				TexCoord2 = texCoord2;
				Offset = offset;
				Width = width;
			}
		}

		private class Counter
		{
			private short[] m_counts = new short[32];

			public int MaxUsedIndex { get; private set; } = -1;


			public void Increment(int i)
			{
				while (i >= m_counts.Length)
				{
					short[] counts = m_counts;
					m_counts = new short[m_counts.Length * 2];
					Array.Copy(counts, m_counts, MaxUsedIndex + 1);
				}
				m_counts[i]++;
				MaxUsedIndex = MathUtils.Max(i, MaxUsedIndex);
			}

			public int Get(int i)
			{
				return m_counts[i];
			}

			public void Clear()
			{
				Array.Clear(m_counts, 0, MaxUsedIndex + 1);
				MaxUsedIndex = -1;
			}
		}

		private static BitmapFont m_debugFont;

		internal Glyph[] m_glyphsByCode;

		internal Dictionary<int, short> m_kerningPairs;

		internal Image m_image;

		public Texture2D Texture { get; private set; }

		public float GlyphHeight { get; private set; }

		public float LineHeight { get; private set; }

		public Vector2 Spacing { get; private set; }

		public float Scale { get; private set; }

		public Glyph FallbackGlyph { get; private set; }

		public char MaxGlyphCode { get; private set; }

		public static BitmapFont DebugFont
		{
			get
			{
				if (m_debugFont == null)
				{
					m_debugFont = BitmapFontContentReader.ReadBitmapFont(typeof(BitmapFont).GetTypeInfo().Assembly.GetManifestResourceStream("Engine.Resources.Embedded.DebugFont.dat"));
				}
				return m_debugFont;
			}
		}

		public BitmapFont(Texture2D texture, IEnumerable<Glyph> glyphs, char fallbackCode, float glyphHeight, Vector2 spacing, float scale)
		{
			Initialize(texture, null, glyphs, fallbackCode, glyphHeight, spacing, scale);
		}

		public void Dispose()
		{
			if (Texture != null)
			{
				Texture.Dispose();
				Texture = null;
			}
		}

		public BitmapFont Clone(float scale, Vector2 spacing)
		{
			return new BitmapFont
			{
				m_glyphsByCode = m_glyphsByCode,
				m_kerningPairs = m_kerningPairs,
				m_image = m_image,
				Texture = Texture,
				GlyphHeight = GlyphHeight,
				LineHeight = LineHeight,
				Spacing = spacing,
				Scale = scale,
				FallbackGlyph = FallbackGlyph,
				MaxGlyphCode = MaxGlyphCode
			};
		}

		public Glyph GetGlyph(char code)
		{
			if (code >= m_glyphsByCode.Length)
			{
				return FallbackGlyph;
			}
			return m_glyphsByCode[(uint)code];
		}

		public Vector2 MeasureText(string text, Vector2 scale, Vector2 spacing)
		{
			return MeasureText(text, 0, text.Length, scale, spacing);
		}

		public Vector2 MeasureText(string text, int start, int count, Vector2 scale, Vector2 spacing)
		{
			scale *= Scale;
			spacing += Spacing;
			Vector2 vector = new Vector2(0f, (GlyphHeight + spacing.Y) * scale.Y);
			Vector2 result = vector;
			int i = start;
			for (int num = start + count; i < num; i++)
			{
				char c = text[i];
				if (c == '\u00a0')
				{
					c = ' ';
				}
				switch (c)
				{
				case '\n':
					vector.X = 0f;
					vector.Y += (GlyphHeight + spacing.Y) * scale.Y;
					if (vector.Y > result.Y)
					{
						result.Y = vector.Y;
					}
					continue;
				case '\r':
				case '\u200b':
					continue;
				}
				Glyph glyph = GetGlyph(c);
				float num2 = ((i < text.Length - 1) ? GetKerning(c, text[i + 1]) : 0f);
				vector.X += (glyph.Width - num2 + spacing.X) * scale.X;
				if (vector.X > result.X)
				{
					result.X = vector.X;
				}
			}
			return result;
		}

		public int FitText(float width, string text, float scale, float spacing)
		{
			return FitText(width, text, 0, text.Length, scale, spacing);
		}

		public int FitText(float width, string text, int start, int count, float scale, float spacing)
		{
			scale *= Scale;
			spacing += Spacing.X;
			float num = 0f;
			for (int i = start; i < start + count; i++)
			{
				char c = text[i];
				if (c == '\u00a0')
				{
					c = ' ';
				}
				switch (c)
				{
				case '\n':
					num = 0f;
					continue;
				case '\r':
				case '\u200b':
					continue;
				}
				Glyph glyph = GetGlyph(c);
				float num2 = ((i < text.Length - 1) ? GetKerning(c, text[i + 1]) : 0f);
				num += (glyph.Width - num2 + spacing) * scale;
				if (num > width)
				{
					return i - start;
				}
			}
			return count;
		}

		public float CalculateCharacterPosition(string text, int characterIndex, Vector2 scale, Vector2 spacing)
		{
			characterIndex = MathUtils.Clamp(characterIndex, 0, text.Length);
			return MeasureText(text, 0, characterIndex, scale, spacing).X;
		}

		public float GetKerning(char code, char followingCode)
		{
			short value = 0;
			if (m_kerningPairs != null)
			{
				m_kerningPairs.TryGetValue((int)(((uint)code << 16) | followingCode), out value);
			}
			return value;
		}

		public void SetKerning(char code, char followingCode, float kerning)
		{
			if (m_kerningPairs == null)
			{
				m_kerningPairs = new Dictionary<int, short>();
			}
			m_kerningPairs[(int)(((uint)code << 16) | followingCode)] = (short)kerning;
		}

		public static BitmapFont Load(Image image, char firstCode, char fallbackCode, Vector2 spacing, float scale, float minWidth, Vector2 offset, KerningSettings kerningSettings = null, int mipLevelsCount = 1, bool premultiplyAlpha = true)
		{
			return InternalLoad(image, firstCode, fallbackCode, spacing, scale, minWidth, offset, kerningSettings, mipLevelsCount, premultiplyAlpha, createTexture: true);
		}

		public static BitmapFont Load(Stream stream, char firstCode, char fallbackCode, Vector2 spacing, float scale, float minWidth, Vector2 offset, KerningSettings kerningSettings = null, int mipLevelsCount = 1, bool premultiplyAlpha = true)
		{
			return Load(Image.Load(stream), firstCode, fallbackCode, spacing, scale, minWidth, offset, kerningSettings, mipLevelsCount, premultiplyAlpha);
		}

		public static BitmapFont Load(string fileName, char firstCode, char fallbackCode, Vector2 spacing, float scale, float minWidth, Vector2 offset, KerningSettings kerningSettings = null, int mipLevelsCount = 1, bool premultiplyAlpha = true)
		{
			using (Stream stream = Storage.OpenFile(fileName, OpenFileMode.Read))
			{
				return Load(stream, firstCode, fallbackCode, spacing, scale, minWidth, offset, kerningSettings, mipLevelsCount, premultiplyAlpha);
			}
		}

		static BitmapFont()
		{
			Display.DeviceReset += delegate
			{
				if (m_debugFont != null)
				{
					BitmapFontContentReader.InitializeBitmapFont(typeof(BitmapFont).GetTypeInfo().Assembly.GetManifestResourceStream("Engine.Resources.Embedded.DebugFont.dat"), m_debugFont);
				}
			};
		}

		internal BitmapFont()
		{
		}

		internal static BitmapFont InternalLoad(Image image, char firstCode, char fallbackCode, Vector2 spacing, float scale, float minWidth, Vector2 offset, KerningSettings kerningSettings, int mipLevelsCount, bool premultiplyAlpha, bool createTexture)
		{
			List<Rectangle> list = new List<Rectangle>(FindGlyphs(image));
			List<Rectangle> list2 = new List<Rectangle>(list.Select((Rectangle r) => CropGlyph(image, r)));
			if (list.Count == 0)
			{
				throw new InvalidOperationException("No glyphs found in BitmapFont image.");
			}
			int num = int.MaxValue;
			int num2 = int.MaxValue;
			int num3 = int.MaxValue;
			int num4 = int.MaxValue;
			for (int i = 0; i < list2.Count; i++)
			{
				if (list2[i].Width > 0 && list2[i].Height > 0)
				{
					num = Math.Min(num, list2[i].Left - list[i].Left);
					num2 = Math.Min(num2, list2[i].Top - list[i].Top);
					num3 = Math.Min(num3, list[i].Right - list2[i].Right);
					num4 = Math.Min(num4, list[i].Bottom - list2[i].Bottom);
				}
			}
			float num5 = 0f;
			List<Glyph> list3 = new List<Glyph>();
			for (int j = 0; j < list.Count; j++)
			{
				Vector2 texCoord;
				Vector2 texCoord2;
				Vector2 offset2;
				if (list2[j].Width > 0 && list2[j].Height > 0)
				{
					texCoord = new Vector2(((float)list2[j].Left - 0.5f) / (float)image.Width, ((float)list2[j].Top - 0.5f) / (float)image.Height);
					texCoord2 = new Vector2(((float)list2[j].Right + 0.5f) / (float)image.Width, ((float)list2[j].Bottom + 0.5f) / (float)image.Height);
					offset2 = new Vector2((float)(list2[j].Left - list[j].Left - num) - 0.5f, (float)(list2[j].Top - list[j].Top - num2) - 0.5f);
				}
				else
				{
					texCoord = Vector2.Zero;
					texCoord2 = Vector2.Zero;
					offset2 = Vector2.Zero;
				}
				offset2 += offset;
				float num6 = list[j].Width - num - num3;
				num5 = MathUtils.Max(num5, list[j].Height - num2 - num4);
				if (num6 < minWidth)
				{
					offset2.X += (minWidth - num6) / 2f;
					num6 = minWidth;
				}
				list3.Add(new Glyph((char)(j + firstCode), texCoord, texCoord2, offset2, num6));
			}
			Image image2 = new Image(image.Width, image.Height);
			for (int k = 0; k < image.Pixels.Length; k++)
			{
				image2.Pixels[k] = ((image.Pixels[k] == Color.Magenta) ? Color.Transparent : image.Pixels[k]);
			}
			if (premultiplyAlpha)
			{
				Image.PremultiplyAlpha(image2);
			}
			Texture2D texture = (createTexture ? Texture2D.Load(image2, mipLevelsCount) : null);
			Image image3 = (createTexture ? null : image2);
			BitmapFont bitmapFont = new BitmapFont();
			bitmapFont.Initialize(texture, image3, list3, fallbackCode, num5, spacing, scale);
			if (kerningSettings != null)
			{
				int[][] array = new int[list.Count][];
				int[][] array2 = new int[list.Count][];
				for (int l = 0; l < list.Count; l++)
				{
					CalculateKerningDepths(image, list2[l], out array[l], out array2[l]);
					array[l] = ApplyKerningBulking(array[l], kerningSettings.BulkingRadius, kerningSettings.BulkingGradient);
					array2[l] = ApplyKerningBulking(array2[l], kerningSettings.BulkingRadius, kerningSettings.BulkingGradient);
				}
				Counter counter = new Counter();
				for (int m = 0; m < list.Count; m++)
				{
					for (int n = 0; n < list.Count; n++)
					{
						int num7 = list2[m].Top - list[m].Top;
						int x = list2[m].Bottom - list[m].Top;
						int num8 = list2[n].Top - list[n].Top;
						int x2 = list2[n].Bottom - list[n].Top;
						int num9 = MathUtils.Max(num7, num8);
						int num10 = MathUtils.Min(x, x2);
						counter.Clear();
						for (int num11 = num9; num11 < num10; num11++)
						{
							int num12 = num11 - num7;
							int num13 = num11 - num8;
							int num14 = array2[m][num12];
							int num15 = array[n][num13];
							counter.Increment(num15 + num14);
						}
						int num16 = MathUtils.Min(kerningSettings.Limit - 1, counter.MaxUsedIndex);
						int tolerance = kerningSettings.Tolerance;
						int num17 = 0;
						int num18;
						for (num18 = 0; num18 <= num16; num18++)
						{
							num17 += counter.Get(num18);
							if (num17 > tolerance)
							{
								break;
							}
						}
						if (num18 != 0)
						{
							bitmapFont.SetKerning((char)(m + firstCode), (char)(n + firstCode), num18);
						}
					}
				}
			}
			return bitmapFont;
		}

		internal void Initialize(Texture2D texture, Image image, IEnumerable<Glyph> glyphs, char fallbackCode, float glyphHeight, Vector2 spacing, float scale)
		{
			Dispose();
			Texture = texture;
			m_image = image;
			GlyphHeight = glyphHeight;
			LineHeight = glyphHeight + spacing.Y;
			Spacing = spacing;
			Scale = scale;
			FallbackGlyph = glyphs.First((Glyph g) => g.Code == fallbackCode);
			MaxGlyphCode = glyphs.Max((Glyph g) => g.Code);
			m_glyphsByCode = new Glyph[MaxGlyphCode + 1];
			for (int i = 0; i < m_glyphsByCode.Length; i++)
			{
				m_glyphsByCode[i] = FallbackGlyph;
			}
			foreach (Glyph glyph in glyphs)
			{
				m_glyphsByCode[(uint)glyph.Code] = glyph;
			}
		}

		private static IEnumerable<Rectangle> FindGlyphs(Image image)
		{
			int y = 1;
			while (y < image.Height)
			{
				int num;
				for (int x = 1; x < image.Width; x = num)
				{
					if (image.GetPixel(x, y) != Color.Magenta && image.GetPixel(x - 1, y) == Color.Magenta && image.GetPixel(x, y - 1) == Color.Magenta)
					{
						int i = 1;
						int j = 1;
						for (; x + i < image.Width && image.GetPixel(x + i, y) != Color.Magenta; i++)
						{
						}
						for (; y + j < image.Height && image.GetPixel(x, y + j) != Color.Magenta; j++)
						{
						}
						yield return new Rectangle(x, y, i, j);
					}
					num = x + 1;
				}
				num = y + 1;
				y = num;
			}
		}

		private static Rectangle CropGlyph(Image image, Rectangle rectangle)
		{
			int num = int.MaxValue;
			int num2 = int.MaxValue;
			int num3 = int.MinValue;
			int num4 = int.MinValue;
			for (int i = rectangle.Left; i < rectangle.Left + rectangle.Width; i++)
			{
				for (int j = rectangle.Top; j < rectangle.Top + rectangle.Height; j++)
				{
					if (image.GetPixel(i, j).A != 0)
					{
						num = MathUtils.Min(num, i);
						num2 = MathUtils.Min(num2, j);
						num3 = MathUtils.Max(num3, i);
						num4 = MathUtils.Max(num4, j);
					}
				}
			}
			if (num == int.MaxValue)
			{
				return new Rectangle(rectangle.Left, rectangle.Top, 0, 0);
			}
			return new Rectangle(num, num2, num3 - num + 1, num4 - num2 + 1);
		}

		private static void CalculateKerningDepths(Image image, Rectangle rectangle, out int[] leftDepths, out int[] rightDepths)
		{
			leftDepths = new int[rectangle.Height];
			rightDepths = new int[rectangle.Height];
			for (int i = rectangle.Top; i < rectangle.Bottom; i++)
			{
				int num = i - rectangle.Top;
				leftDepths[num] = rectangle.Width;
				rightDepths[num] = rectangle.Width;
				for (int j = rectangle.Left; j < rectangle.Right; j++)
				{
					if (image.GetPixel(j, i).A != 0)
					{
						leftDepths[num] = MathUtils.Min(leftDepths[num], j - rectangle.Left);
						rightDepths[num] = MathUtils.Min(rightDepths[num], rectangle.Right - j - 1);
					}
				}
			}
		}

		private static int[] ApplyKerningBulking(int[] depths, int radius, float gradient)
		{
			int[] array = new int[depths.Length];
			for (int i = 0; i < depths.Length; i++)
			{
				array[i] = depths[i];
				int num = MathUtils.Max(i - radius, 0);
				int num2 = MathUtils.Min(i + radius, depths.Length - 1);
				for (int j = num; j <= num2; j++)
				{
					int num3 = Math.Abs(j - i);
					int x = depths[j] + (int)Math.Round(gradient * (float)num3);
					array[i] = MathUtils.Min(array[i], x);
				}
			}
			return array;
		}
	}
}
