using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;

namespace Engine.Media
{
    public class Image
    {
        public static IImageFormatConfigurationModule[] ImageSharpModules = [
            new SixLabors.ImageSharp.Formats.Bmp.BmpConfigurationModule(),
            new SixLabors.ImageSharp.Formats.Gif.GifConfigurationModule(),
            new SixLabors.ImageSharp.Formats.Jpeg.JpegConfigurationModule(),
            new SixLabors.ImageSharp.Formats.Pbm.PbmConfigurationModule(),
            new SixLabors.ImageSharp.Formats.Png.PngConfigurationModule(),
            new SixLabors.ImageSharp.Formats.Qoi.QoiConfigurationModule(),
            new SixLabors.ImageSharp.Formats.Tga.TgaConfigurationModule(),
            new SixLabors.ImageSharp.Formats.Tiff.TiffConfigurationModule(),
            new SixLabors.ImageSharp.Formats.Webp.WebpConfigurationModule()
            ];
        public static Configuration DefaultImageSharpConfiguration = new Configuration(ImageSharpModules) { PreferContiguousImageBuffers = true };
        public static DecoderOptions DefaultImageSharpDecoderOptions = new DecoderOptions() { Configuration = DefaultImageSharpConfiguration};

        public int Width => m_trueImage.Width;

        public int Height => m_trueImage.Height;

        public Color[] m_pixels = null;
        public bool m_shouldUpdatePixelsCache = true;

        public Color[] Pixels
        {
            get
            {
                if(m_pixels == null || m_shouldUpdatePixelsCache)
                {
                    m_pixels = new Color[Width * Height];
                    int i = 0;
                    m_trueImage.ProcessPixelRows(
                        accessor => {
                            Span<Color> pixelsSpan = m_pixels.AsSpan();
                            for (int y = 0; y < accessor.Height; y++)
                            {
                                MemoryMarshal.Cast<Rgba32, Color>(accessor.GetRowSpan(y)).CopyTo(pixelsSpan.Slice(y * Width, Width));
                            }
                        }
                    );
                }
                m_shouldUpdatePixelsCache = false;
                return m_pixels;
            }
        }

        public readonly Image<Rgba32> m_trueImage;

        public Image(Image image)
        {
            ArgumentNullException.ThrowIfNull(image);
            m_trueImage = image.m_trueImage.Clone();
        }

        public Image(Image<Rgba32> image)
        {
            ArgumentNullException.ThrowIfNull(image);
            m_trueImage = image;
        }
        public Image(LegacyImage image)
        {
            ArgumentNullException.ThrowIfNull(image);
            m_trueImage = new Image<Rgba32>(DefaultImageSharpConfiguration, image.Width, image.Height);
            m_trueImage.ProcessPixelRows(
                accessor => {
                    Span<Color> pixels = image.Pixels.AsSpan();
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        MemoryMarshal.Cast<Color, Rgba32>(pixels.Slice(y * image.Width, image.Height)).CopyTo(accessor.GetRowSpan(y));
                    }
                }
            );
        }

        public Image(int width, int height)
        {
            if (width < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }
            if (height < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }
            m_trueImage = new Image<Rgba32>(DefaultImageSharpConfiguration, width, height);
        }

        public Color GetPixel(int x, int y)
        {
            if (x < 0 || x >= Width)
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }
            if (y < 0 || y >= Height)
            {
                throw new ArgumentOutOfRangeException(nameof(y));
            }
            return new Color(m_trueImage[x,y].PackedValue);
        }

        public void SetPixel(int x, int y, Color color)
        {
            if (x < 0 || x >= Width)
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }
            if (y < 0 || y >= Height)
            {
                throw new ArgumentOutOfRangeException(nameof(y));
            }
            m_trueImage[x, y] = new Rgba32(color.PackedValue);
            m_shouldUpdatePixelsCache = true;
        }

        public static void PremultiplyAlpha(Image image)
        {
            image.m_trueImage.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    foreach (ref Rgba32 pixel in accessor.GetRowSpan(y))
                    {
                        pixel = pixel.PremultiplyAlpha();
                    }
                }
            });
            image.m_shouldUpdatePixelsCache = true;
        }

        public static ImageFileFormat DetermineFileFormat(string extension) => Name2EngineImageFormat.TryGetValue(extension.Substring(1).ToLower(), out ImageFileFormat format)
                ? format
                : throw new InvalidOperationException("Unsupported image file format.");

        public static ImageFileFormat DetermineFileFormat(Stream stream) => Name2EngineImageFormat.TryGetValue(SixLabors.ImageSharp.Image.Identify(stream).Metadata.DecodedImageFormat.Name.ToLower(), out ImageFileFormat format)
                ? format
                : throw new InvalidOperationException("Unsupported image file format.");

        public static Image Load(Stream stream, ImageFileFormat format) => Name2EngineImageFormat.TryGetValue(SixLabors.ImageSharp.Image.Identify(stream).Metadata.DecodedImageFormat.Name.ToLower(), out ImageFileFormat IdentifiedFormat) && IdentifiedFormat == format
                ? Load(stream)
                : throw new FormatException($"Image format({IdentifiedFormat}) is not ${format}");

        public static Image Load(string fileName, ImageFileFormat format)
        {
            using (Stream stream = Storage.OpenFile(fileName, OpenFileMode.Read))
            {
                return Load(stream, format);
            }
        }

        public static Image Load(Stream stream)
        {
            return new Image(SixLabors.ImageSharp.Image.Load<Rgba32>(DefaultImageSharpDecoderOptions, stream));
        }

        public static Image Load(string fileName)
        {
            using (Stream stream = Storage.OpenFile(fileName, OpenFileMode.Read))
            {
                return Load(stream);
            }
        }

        public static void Save(Image image, Stream stream, ImageFileFormat format, bool saveAlpha)
        {
            //Todo
            switch (format)
            {
                case ImageFileFormat.Bmp:
                    Bmp.Save(image, stream, (!saveAlpha) ? Bmp.Format.RGB8 : Bmp.Format.RGBA8);
                    break;
                case ImageFileFormat.Png:
                    Png.Save(image, stream, (!saveAlpha) ? Png.Format.RGB8 : Png.Format.RGBA8);
                    break;
                case ImageFileFormat.Jpg:
                    Jpg.Save(image, stream, 95);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported image file format.");
            }
        }

        public static void Save(Image image, string fileName, ImageFileFormat format, bool saveAlpha)
        {
            using (Stream stream = Storage.OpenFile(fileName, OpenFileMode.Create))
            {
                Save(image, stream, format, saveAlpha);
            }
        }
        public readonly static Dictionary<string, ImageFileFormat> Name2EngineImageFormat = new()
        {
            {"bmp", ImageFileFormat.Bmp },
            {"png", ImageFileFormat.Png },
            {"jpg", ImageFileFormat.Jpg },
            {"jpeg", ImageFileFormat.Jpg },
            {"gif", ImageFileFormat.Gif },
            {"pbm", ImageFileFormat.Pbm },
            {"tiff", ImageFileFormat.Tiff },
            {"tga", ImageFileFormat.Tga },
            {"webp", ImageFileFormat.WebP }
        };
    }
}
