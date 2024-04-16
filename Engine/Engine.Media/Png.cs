using Hjg.Pngcs;
using Hjg.Pngcs.Chunks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.IO;

namespace Engine.Media
{
	public static class Png
	{
		public enum Format
		{
			RGBA8,
			RGB8,
			L8,
			LA8,
			Indexed
		}

		public struct PngInfo
		{
			public int Width;

			public int Height;

			public Format Format;
		}

		public static bool IsPngStream(Stream stream)
		{
			ArgumentNullException.ThrowIfNull(stream);
			return SixLabors.ImageSharp.Image.DetectFormat(stream).Name == "PNG";
        }

		public static PngInfo GetInfo(Stream stream)
		{
			ArgumentNullException.ThrowIfNull(stream);
            SixLabors.ImageSharp.ImageInfo info = SixLabors.ImageSharp.Image.Identify(stream);
			if(info.Metadata.DecodedImageFormat.Name != "PNG")
			{
				throw new FormatException($"Image format({info.Metadata.DecodedImageFormat.Name}) is not Png");
			}
            PngInfo result = default;
			result.Width = info.Width;
			result.Height = info.Height;
			switch (info.Metadata.GetPngMetadata().ColorType)
			{
				case SixLabors.ImageSharp.Formats.Png.PngColorType.RgbWithAlpha:
                    result.Format = Format.RGBA8;
					break;
				case SixLabors.ImageSharp.Formats.Png.PngColorType.Rgb:
                    result.Format = Format.RGB8;
					break;
				case SixLabors.ImageSharp.Formats.Png.PngColorType.GrayscaleWithAlpha:
                    result.Format = Format.LA8;
					break;
				case SixLabors.ImageSharp.Formats.Png.PngColorType.Grayscale:
					result.Format = Format.L8;
					break;
				case SixLabors.ImageSharp.Formats.Png.PngColorType.Palette:
					result.Format = Format.Indexed;
					break;
				default:
                    throw new InvalidOperationException("Unsupported PNG pixel format.");
            }
			return result;
		}

		public static Image Load(Stream stream)
		{
			ArgumentNullException.ThrowIfNull(stream);
            string formatName = SixLabors.ImageSharp.Image.DetectFormat(stream).Name;
            if (formatName != "PNG")
            {
                throw new FormatException($"Image format({formatName}) is not Png");
            }
			return Image.Load(stream);
		}

		public static void Save(Image image, Stream stream, Format format, PngCompressionLevel compressionLevel = PngCompressionLevel.DefaultCompression, bool sync = false)
		{
			ArgumentNullException.ThrowIfNull(image);
			ArgumentNullException.ThrowIfNull(stream);
			PngColorType pngColorType;
			switch (format)
			{
				case Format.RGBA8:
					pngColorType = PngColorType.RgbWithAlpha;
					break;
				case Format.RGB8:
					pngColorType = PngColorType.Rgb;
					break;
				case Format.LA8:
					pngColorType = PngColorType.GrayscaleWithAlpha;
					break;
				case Format.L8:
					pngColorType = PngColorType.Grayscale;
					break;
				case Format.Indexed:
					pngColorType = PngColorType.Palette;
					break;
                default:
                    throw new InvalidOperationException("Unsupported PNG pixel format.");
            }
            PngEncoder encoder = new PngEncoder()
			{
				ColorType = pngColorType,
				CompressionLevel = compressionLevel,
				TransparentColorMode = PngTransparentColorMode.Clear
			};
			if (sync) {
                image.m_trueImage.SaveAsPng(stream, encoder);
            }
			else
			{
				image.m_trueImage.SaveAsPngAsync(stream, encoder);
			}
		}
	}
}
