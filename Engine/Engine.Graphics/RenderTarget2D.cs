using Engine.Media;
using OpenTK.Graphics.ES30;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Engine.Graphics
{
    public  class RenderTarget2D : Texture2D
    {
        public int m_frameBuffer;

        public int m_depthBuffer;

        public DepthFormat DepthFormat
        {
            get;
            set;
        }

        public RenderTarget2D(int width, int height, int mipLevelsCount, ColorFormat colorFormat, DepthFormat depthFormat)
            : base(width, height, mipLevelsCount, colorFormat)
        {
            try
            {
                InitializeRenderTarget2D(width, height, mipLevelsCount, colorFormat, depthFormat);
                AllocateRenderTarget();
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            DeleteRenderTarget();
        }

        public void GetData<T>(T[] target, int targetStartIndex, Rectangle sourceRectangle) where T : struct
        {
            VerifyParametersGetData(target, targetStartIndex, sourceRectangle);
            var gCHandle = GCHandle.Alloc(target, GCHandleType.Pinned);
            try
            {
                GLWrapper.BindFramebuffer(m_frameBuffer);
                GL.ReadPixels(sourceRectangle.Left, sourceRectangle.Top, sourceRectangle.Width, sourceRectangle.Height, PixelFormat.Rgba, PixelType.UnsignedByte, gCHandle.AddrOfPinnedObject());
            }
            finally
            {
                gCHandle.Free();
            }
        }
        public unsafe Image GetData(Rectangle sourceRectangle)
        {
            VerifyNotDisposed();
            SixLabors.ImageSharp.Image<Rgba32> image = new(Image.DefaultImageSharpConfiguration, sourceRectangle.Width, sourceRectangle.Height);
            image.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> memory);
            GLWrapper.BindFramebuffer(m_frameBuffer);
            GL.ReadPixels(sourceRectangle.Left, sourceRectangle.Top, sourceRectangle.Width, sourceRectangle.Height, PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr)memory.Pin().Pointer);
            return new Image(image);
        }

        public void GenerateMipMaps()
        {
            GLWrapper.BindTexture(TextureTarget.Texture2D, m_texture, forceBind: false);
            GL.GenerateMipmap(TextureTarget.Texture2D);
        }

        internal override void HandleDeviceLost()
        {
            DeleteRenderTarget();
        }

        internal override void HandleDeviceReset()
        {
            AllocateRenderTarget();
        }

        public void AllocateRenderTarget()
        {
            GL.GenFramebuffers(1, out m_frameBuffer);
            GLWrapper.BindFramebuffer(m_frameBuffer);
            GL.FramebufferTexture2D(All.Framebuffer, All.ColorAttachment0, All.Texture2D, m_texture, 0);
            if (DepthFormat != 0)
            {
                GL.GenRenderbuffers(1, out m_depthBuffer);
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, m_depthBuffer);
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, GLWrapper.TranslateDepthFormat(DepthFormat), base.Width, base.Height);
                GL.FramebufferRenderbuffer(All.Framebuffer, All.DepthAttachment, All.Renderbuffer, m_depthBuffer);
                GL.FramebufferRenderbuffer(All.Framebuffer, All.StencilAttachment, All.Renderbuffer, 0);
            }
            else
            {
                GL.FramebufferRenderbuffer(All.Framebuffer, All.DepthAttachment, All.Renderbuffer, 0);
                GL.FramebufferRenderbuffer(All.Framebuffer, All.StencilAttachment, All.Renderbuffer, 0);
            }
            FramebufferErrorCode framebufferErrorCode = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (framebufferErrorCode != FramebufferErrorCode.FramebufferComplete)
            {
                throw new InvalidOperationException($"Error creating framebuffer ({framebufferErrorCode.ToString()}).");
            }
        }

        public void DeleteRenderTarget()
        {
            if (m_depthBuffer != 0)
            {
                GL.DeleteRenderbuffers(1, ref m_depthBuffer);
                m_depthBuffer = 0;
            }
            if (m_frameBuffer != 0)
            {
                GLWrapper.DeleteFramebuffer(m_frameBuffer);
                m_frameBuffer = 0;
            }
        }

		public static void Save(RenderTarget2D renderTarget, Stream stream, ImageFileFormat format, bool saveAlpha)
		{
			if (renderTarget.ColorFormat != ColorFormat.Rgba8888)
			{
				throw new InvalidOperationException("Unsupported color format.");
			}
			Image.Save(renderTarget.GetData(new Rectangle(0, 0, renderTarget.Width, renderTarget.Height)), stream, format, saveAlpha);
		}

		public override int GetGpuMemoryUsage()
		{
			return base.GetGpuMemoryUsage() + (DepthFormat.GetSize() * base.Width * base.Height);
		}

		private void InitializeRenderTarget2D(int width, int height, int mipLevelsCount, ColorFormat colorFormat, DepthFormat depthFormat)
		{
			DepthFormat = depthFormat;
		}

		private void VerifyParametersGetData<T>(T[] target, int targetStartIndex, Rectangle sourceRectangle) where T : struct
		{
			VerifyNotDisposed();
			int size = base.ColorFormat.GetSize();
			int num = Utilities.SizeOf<T>();
			ArgumentNullException.ThrowIfNull(target);
			if (num > size)
			{
				throw new ArgumentNullException("Target array element size is larger than pixel size.");
			}
			if (size % num != 0)
			{
				throw new ArgumentNullException("Pixel size is not an integer multiple of target array element size.");
			}
			if (sourceRectangle.Left < 0 || sourceRectangle.Width <= 0 || sourceRectangle.Top < 0 || sourceRectangle.Height <= 0 || sourceRectangle.Left + sourceRectangle.Width > base.Width || sourceRectangle.Top + sourceRectangle.Height > base.Height)
			{
				throw new ArgumentOutOfRangeException("sourceRectangle");
			}
			if (targetStartIndex < 0 || targetStartIndex >= target.Length)
			{
				throw new ArgumentOutOfRangeException("targetStartIndex");
			}
			if ((target.Length - targetStartIndex) * num < sourceRectangle.Width * sourceRectangle.Height * size)
			{
				throw new InvalidOperationException("Not enough space in target array.");
			}
		}
    }
}
