using SixLabors.ImageSharp.PixelFormats;

namespace Engine.Media
{
    public static class Rgba32Extensions
    {
        public static Rgba32 PremultiplyAlpha(this Rgba32 c)
        {
            return new Rgba32((byte)((float)(c.R * c.A) / 255f), (byte)((float)(c.G * c.A) / 255f), (byte)((float)(c.B * c.A) / 255f), c.A);
        }
    }
}
