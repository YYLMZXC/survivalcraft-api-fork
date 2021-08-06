using Engine.Graphics;
using Engine.Media;
using System;
using System.Xml.Linq;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

namespace Engine.Serialization
{
    public class Subtexture
    {
        public readonly Texture2D Texture;

        public readonly Vector2 TopLeft;

        public readonly Vector2 BottomRight;

        public Subtexture(Texture2D texture, Vector2 topLeft, Vector2 bottomRight)
        {
            Texture = texture;
            TopLeft = topLeft;
            BottomRight = bottomRight;
        }
    }
    public class TextureAtlas
    {
        public Texture2D m_texture;

        public Dictionary<string, Rectangle> m_rectangles = new Dictionary<string, Rectangle>();

        public Texture2D Texture => m_texture;

        public TextureAtlas(Texture2D texture, string atlasDefinition, string prefix)
        {
            m_texture = texture;
            string[] array = atlasDefinition.Split(new char[2]
            {
                '\n',
                '\r'
            }, StringSplitOptions.RemoveEmptyEntries);
            int num = 0;
            while (true)
            {
                if (num < array.Length)
                {
                    string[] array2 = array[num].Split(new char[1]
                    {
                        ' '
                    }, StringSplitOptions.RemoveEmptyEntries);
                    if (array2.Length < 5)
                    {
                        break;
                    }
                    string key = prefix + array2[0];
                    var value = new Rectangle
                    {
                        Left = int.Parse(array2[1], CultureInfo.InvariantCulture),
                        Top = int.Parse(array2[2], CultureInfo.InvariantCulture),
                        Width = int.Parse(array2[3], CultureInfo.InvariantCulture),
                        Height = int.Parse(array2[4], CultureInfo.InvariantCulture)
                    };
                    m_rectangles.Add(key, value);
                    num++;
                    continue;
                }
                return;
            }
            throw new InvalidOperationException("Invalid texture atlas definition.");
        }

        public bool ContainsTexture(string textureName)
        {
            return m_rectangles.ContainsKey(textureName);
        }

        public Vector4? GetTextureCoordinates(string textureName)
        {
            if (m_rectangles.TryGetValue(textureName, out Rectangle value))
            {
                Vector4 value2 = default;
                value2.X = value.Left / (float)m_texture.Width;
                value2.Y = value.Top / (float)m_texture.Height;
                value2.Z = value.Right / (float)m_texture.Width;
                value2.W = value.Bottom / (float)m_texture.Height;
                return value2;
            }
            return null;
        }
    }
    public static class TextureAtlasManager
    {
        public static Dictionary<string, Subtexture> m_subtextures = new Dictionary<string, Subtexture>();
        public static Texture2D AtlasTexture;
        public static void LoadAtlases(Texture2D AtlasTexture_,string Atlas)
        {
            m_subtextures.Clear();
            AtlasTexture = AtlasTexture_;
            LoadTextureAtlas(AtlasTexture, Atlas, "Textures/Atlas/");
        }

        public static Subtexture GetSubtexture(string name)
        {
            if (!m_subtextures.TryGetValue(name, out Subtexture value))
            {
                try
                {
                    value = new Subtexture(AtlasTexture, Vector2.Zero, Vector2.One);
                    m_subtextures.Add(name, value);
                    return value;
                }
                catch (Exception innerException)
                {
                    throw new InvalidOperationException($"Required subtexture {name} not found in TextureAtlasManager.", innerException);
                }
            }
            return value;
        }

        public static void LoadTextureAtlas(Texture2D texture, string atlasDefinition, string prefix)
        {
            string[] array = atlasDefinition.Split(new char[2]
            {
                '\n',
                '\r'
            }, StringSplitOptions.RemoveEmptyEntries);
            int num = 0;
            while (true)
            {
                if (num < array.Length)
                {
                    string[] array2 = array[num].Split(new char[1]
                    {
                        ' '
                    }, StringSplitOptions.RemoveEmptyEntries);
                    if (array2.Length < 5)
                    {
                        break;
                    }
                    string key = prefix + array2[0];
                    int num2 = int.Parse(array2[1], CultureInfo.InvariantCulture);
                    int num3 = int.Parse(array2[2], CultureInfo.InvariantCulture);
                    int num4 = int.Parse(array2[3], CultureInfo.InvariantCulture);
                    int num5 = int.Parse(array2[4], CultureInfo.InvariantCulture);
                    var topLeft = new Vector2(num2 / (float)texture.Width, num3 / (float)texture.Height);
                    var bottomRight = new Vector2((num2 + num4) / (float)texture.Width, (num3 + num5) / (float)texture.Height);
                    var value = new Subtexture(texture, topLeft, bottomRight);
                    m_subtextures.Add(key, value);
                    num++;
                    continue;
                }
                return;
            }
            throw new InvalidOperationException("Invalid texture atlas definition.");
        }
    }
    public class ContentSerializer
    {
        public static object StreamConvertType(Type type, string name, Stream stream)
        {
            switch (type.FullName)
            {
                case "Game.Subtexture": return TextureAtlasManager.GetSubtexture(name);
                case "Engine.Graphics.Texture2D": return Texture2D.Load(stream);
                case "System.String":
                    {
                        break;
                    }
                case "Engine.Media.BitmapFont":
                    {
                        BitmapFont bitmapFont = new BitmapFont();
                        return bitmapFont;
                    }
                case "Engine.Media.Image": return Image.Load(stream);
                case "Engine.Graphics.Shader": { break; }
                case "System.Xml.Linq.XElement": return XElement.Load(stream);
                case "Engine.Graphics.Model": { break; }
            }
            return null;
        }
    }
}
