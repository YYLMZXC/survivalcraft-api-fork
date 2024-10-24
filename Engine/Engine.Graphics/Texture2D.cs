using System.Runtime.InteropServices;
using Engine.Media;
using OpenTK.Graphics.ES30;
using SixLabors.ImageSharp.PixelFormats;

namespace Engine.Graphics
{
	public class Texture2D : GraphicsResource
	{
		public int m_texture;
		public PixelFormat m_pixelFormat;
		public PixelType m_pixelType;

		public IntPtr NativeHandle => m_texture;

		public string DebugName
		{
			get
			{
				return string.Empty;
			}
			set
			{
			}
		}

		public int Width
		{
			get;
			set;
		}

		public int Height
		{
			get;
			set;
		}

		public ColorFormat ColorFormat
		{
			get;
			set;
		}

		public int MipLevelsCount
		{
			get;
			set;
		}

		public object Tag
		{
			get;
			set;
		}

		public Texture2D(int width, int height, int mipLevelsCount, ColorFormat colorFormat)
		{
			InitializeTexture2D(width, height, mipLevelsCount, colorFormat);
			switch (ColorFormat)
			{
				case ColorFormat.Rgba8888:
					m_pixelFormat = PixelFormat.Rgba;
					m_pixelType = PixelType.UnsignedByte;
					break;
				case ColorFormat.Rgb565:
					m_pixelFormat = PixelFormat.Rgb;
					m_pixelType = PixelType.UnsignedShort565;
					break;
				case ColorFormat.Rgba5551:
					m_pixelFormat = PixelFormat.Rgba;
					m_pixelType = PixelType.UnsignedShort5551;
					break;
				case ColorFormat.R8:
					m_pixelFormat = PixelFormat.Luminance;
					m_pixelType = PixelType.UnsignedByte;
					break;
				default:
					throw new InvalidOperationException("Unsupported surface format.");
			}
			AllocateTexture();
		}

		public override void Dispose()
		{
			base.Dispose();
			DeleteTexture();
		}

		public void SetData<T>(int mipLevel, T[] source, int sourceStartIndex = 0) where T : struct
		{
			VerifyParametersSetData(mipLevel, source, sourceStartIndex);
			var gCHandle = GCHandle.Alloc(source, GCHandleType.Pinned);
			try
			{
				int width = MathUtils.Max(Width >> mipLevel, 1);
				int height = MathUtils.Max(Height >> mipLevel, 1);
				IntPtr pixels = gCHandle.AddrOfPinnedObject() + (sourceStartIndex * Utilities.SizeOf<T>());
				GLWrapper.BindTexture(TextureTarget.Texture2D, m_texture, forceBind: false);
				GL.TexImage2D(TextureTarget2d.Texture2D, mipLevel, (TextureComponentCount)m_pixelFormat, width, height, 0, m_pixelFormat, m_pixelType, pixels);
			}
			finally
			{
				gCHandle.Free();
			}
		}
		public unsafe void SetData(SixLabors.ImageSharp.Image<Rgba32> source)
		{
			VerifyParametersSetData(source);
			source.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> memory);
			GLWrapper.BindTexture(TextureTarget.Texture2D, m_texture, false);
			GL.TexImage2D(
				TextureTarget2d.Texture2D,
				0,
				(TextureComponentCount)m_pixelFormat,
				source.Width,
				source.Height,
				0,
				m_pixelFormat,
				m_pixelType,
				(IntPtr)memory.Pin().Pointer
			);
		}

		internal override void HandleDeviceLost()
		{
			DeleteTexture();
		}

		internal override void HandleDeviceReset()
		{
			AllocateTexture();
		}

		public void AllocateTexture()
		{
			GL.GenTextures(1, out m_texture);
			GLWrapper.BindTexture(TextureTarget.Texture2D, m_texture, forceBind: false);
			for (int i = 0; i < MipLevelsCount; i++)
			{
				int width = MathUtils.Max(Width >> i, 1);
				int height = MathUtils.Max(Height >> i, 1);
				GL.TexImage2D(TextureTarget2d.Texture2D, i, (TextureComponentCount)m_pixelFormat, width, height, 0, m_pixelFormat, m_pixelType, IntPtr.Zero);
			}
		}

		public void DeleteTexture()
		{
			if (m_texture != 0)
			{
				GLWrapper.DeleteTexture(m_texture);
				m_texture = 0;
			}
		}
		
		public override int GetGpuMemoryUsage()
		{
			int num = 0;
			for (int i = 0; i < MipLevelsCount; i++)
			{
				int num2 = MathUtils.Max(Width >> i, 1);
				int num3 = MathUtils.Max(Height >> i, 1);
				num += ColorFormat.GetSize() * num2 * num3;
			}
			return num;
		}

		public static Texture2D Load(LegacyImage image, int mipLevelsCount = 1)
		{
			var texture2D = new Texture2D(image.Width, image.Height, mipLevelsCount, ColorFormat.Rgba8888);
			if (mipLevelsCount > 1)
			{
                LegacyImage[] array = LegacyImage.GenerateMipmaps(image, mipLevelsCount).ToArray();
				for (int i = 0; i < array.Length; i++)
				{
					texture2D.SetData(i, array[i].Pixels);
				}
			}
			else
			{
			    texture2D.SetData(0, image.Pixels);
                if (mipLevelsCount > 1)
                {
                    GLWrapper.BindTexture(TextureTarget.Texture2D, texture2D.m_texture, forceBind: false);
                    GL.GenerateMipmap(TextureTarget.Texture2D);
                }
            }
            texture2D.Tag = image;
			return texture2D;
		}

		public static Texture2D Load(Image image, int mipLevelsCount = 1)
		{
			var texture2D = new Texture2D(image.Width, image.Height, mipLevelsCount, ColorFormat.Rgba8888);
			texture2D.SetData(image.m_trueImage);
            if(mipLevelsCount > 1)
            {
                GLWrapper.BindTexture(TextureTarget.Texture2D, texture2D.m_texture, forceBind: false);
                GL.GenerateMipmap(TextureTarget.Texture2D);
            }
            texture2D.Tag = image;
			return texture2D;
		}

        public static Texture2D Load(SixLabors.ImageSharp.Image<Rgba32> image, int mipLevelsCount = 1)
        {
            var texture2D = new Texture2D(image.Width, image.Height, mipLevelsCount, ColorFormat.Rgba8888);
            texture2D.SetData(image);
            if (mipLevelsCount > 1)
            {
                GLWrapper.BindTexture(TextureTarget.Texture2D, texture2D.m_texture, forceBind: false);
                GL.GenerateMipmap(TextureTarget.Texture2D);
            }
            texture2D.Tag = new Image(image);
            return texture2D;
        }

        public static Texture2D Load(Stream stream, bool premultiplyAlpha = false, int mipLevelsCount = 1)
		{
			var image = Image.Load(stream);
			if (premultiplyAlpha)
			{
				Image.PremultiplyAlpha(image);
			}
			return Load(image, mipLevelsCount);
		}

		public static Texture2D Load(string fileName, bool premultiplyAlpha = false, int mipLevelsCount = 1)
		{
			using (Stream stream = Storage.OpenFile(fileName, OpenFileMode.Read))
			{
				return Load(stream, premultiplyAlpha, mipLevelsCount);
			}
		}

		internal void InitializeTexture2D(int width, int height, int mipLevelsCount, ColorFormat colorFormat)
		{
			if (width < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(width));
			}
			if (height < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(height));
			}
			if (mipLevelsCount < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(mipLevelsCount));
			}
			Width = width;
			Height = height;
			ColorFormat = colorFormat;
			if (mipLevelsCount > 1)
			{
				int num = 0;
				for (int num2 = MathUtils.Max(width, height); num2 >= 1; num2 /= 2)
				{
					num++;
				}
				MipLevelsCount = MathUtils.Min(num, mipLevelsCount);
			}
			else
			{
				MipLevelsCount = 1;
			}
		}

		private void VerifyParametersSetData<T>(int mipLevel, T[] source, int sourceStartIndex = 0) where T : struct
		{
			VerifyNotDisposed();
			int num = Utilities.SizeOf<T>();
			int size = ColorFormat.GetSize();
			int num2 = MathUtils.Max(Width >> mipLevel, 1);
			int num3 = MathUtils.Max(Height >> mipLevel, 1);
			int num4 = size * num2 * num3;
			ArgumentNullException.ThrowIfNull(source);
						if (mipLevel < 0 || mipLevel >= MipLevelsCount)
			{
				throw new ArgumentOutOfRangeException(nameof(mipLevel));
			}
			if (num > size)
			{
				throw new ArgumentNullException("Source array element size is larger than pixel size.");
			}
			if (size % num != 0)
			{
				throw new ArgumentNullException("Pixel size is not an integer multiple of source array element size.");
			}
			if (sourceStartIndex < 0 || (source.Length - sourceStartIndex) * num < num4)
			{
				throw new InvalidOperationException("Not enough data in source array.");
			}
		}
		private void VerifyParametersSetData(SixLabors.ImageSharp.Image<Rgba32> source)
		{
			VerifyNotDisposed();
			ArgumentNullException.ThrowIfNull(source);
		}
	}
}
