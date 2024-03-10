namespace Engine.Graphics
{
	public sealed class BlendState : LockOnFirstUse
	{
		private BlendFunction m_alphaBlendFunction;

		private Blend m_alphaSourceBlend = Blend.One;

		private Blend m_alphaDestinationBlend;

		private BlendFunction m_colorBlendFunction;

		private Blend m_colorSourceBlend = Blend.One;

		private Blend m_colorDestinationBlend;

		private Vector4 m_blendFactor = Vector4.Zero;

		public static readonly BlendState Opaque = new()
		{
			IsLocked = true
		};

		public static readonly BlendState Additive = new()
		{
			ColorSourceBlend = Blend.SourceAlpha,
			ColorDestinationBlend = Blend.One,
			AlphaSourceBlend = Blend.SourceAlpha,
			AlphaDestinationBlend = Blend.One,
			IsLocked = true
		};

		public static readonly BlendState AlphaBlend = new()
		{
			ColorSourceBlend = Blend.One,
			ColorDestinationBlend = Blend.InverseSourceAlpha,
			AlphaSourceBlend = Blend.One,
			AlphaDestinationBlend = Blend.InverseSourceAlpha,
			IsLocked = true
		};

		public static readonly BlendState NonPremultiplied = new()
		{
			ColorSourceBlend = Blend.SourceAlpha,
			ColorDestinationBlend = Blend.InverseSourceAlpha,
			AlphaSourceBlend = Blend.SourceAlpha,
			AlphaDestinationBlend = Blend.InverseSourceAlpha,
			IsLocked = true
		};

		public BlendFunction AlphaBlendFunction
		{
			get
			{
				return m_alphaBlendFunction;
			}
			set
			{
				ThrowIfLocked();
				m_alphaBlendFunction = value;
			}
		}

		public Blend AlphaSourceBlend
		{
			get
			{
				return m_alphaSourceBlend;
			}
			set
			{
				ThrowIfLocked();
				m_alphaSourceBlend = value;
			}
		}

		public Blend AlphaDestinationBlend
		{
			get
			{
				return m_alphaDestinationBlend;
			}
			set
			{
				ThrowIfLocked();
				m_alphaDestinationBlend = value;
			}
		}

		public BlendFunction ColorBlendFunction
		{
			get
			{
				return m_colorBlendFunction;
			}
			set
			{
				ThrowIfLocked();
				m_colorBlendFunction = value;
			}
		}

		public Blend ColorSourceBlend
		{
			get
			{
				return m_colorSourceBlend;
			}
			set
			{
				ThrowIfLocked();
				m_colorSourceBlend = value;
			}
		}

		public Blend ColorDestinationBlend
		{
			get
			{
				return m_colorDestinationBlend;
			}
			set
			{
				ThrowIfLocked();
				m_colorDestinationBlend = value;
			}
		}

		public Vector4 BlendFactor
		{
			get
			{
				return m_blendFactor;
			}
			set
			{
				ThrowIfLocked();
				m_blendFactor = value;
			}
		}
	}
}
