namespace Engine.Graphics
{
	public sealed class RasterizerState : LockOnFirstUse
	{
		private CullMode m_cullMode = CullMode.CullCounterClockwise;

		private bool m_scissorTestEnable;

		private float m_depthBias;

		private float m_slopeScaleDepthBias;

		public static readonly RasterizerState CullNone = new()
		{
			CullMode = CullMode.None,
			IsLocked = true
		};

		public static readonly RasterizerState CullNoneScissor = new()
		{
			CullMode = CullMode.None,
			ScissorTestEnable = true,
			IsLocked = true
		};

		public static readonly RasterizerState CullClockwise = new()
		{
			CullMode = CullMode.CullClockwise,
			IsLocked = true
		};

		public static readonly RasterizerState CullClockwiseScissor = new()
		{
			CullMode = CullMode.CullClockwise,
			ScissorTestEnable = true,
			IsLocked = true
		};

		public static readonly RasterizerState CullCounterClockwise = new()
		{
			CullMode = CullMode.CullCounterClockwise,
			IsLocked = true
		};

		public static readonly RasterizerState CullCounterClockwiseScissor = new()
		{
			CullMode = CullMode.CullCounterClockwise,
			ScissorTestEnable = true,
			IsLocked = true
		};

		public CullMode CullMode
		{
			get
			{
				return m_cullMode;
			}
			set
			{
				ThrowIfLocked();
				m_cullMode = value;
			}
		}

		public bool ScissorTestEnable
		{
			get
			{
				return m_scissorTestEnable;
			}
			set
			{
				ThrowIfLocked();
				m_scissorTestEnable = value;
			}
		}

		public float DepthBias
		{
			get
			{
				return m_depthBias;
			}
			set
			{
				ThrowIfLocked();
				m_depthBias = value;
			}
		}

		public float SlopeScaleDepthBias
		{
			get
			{
				return m_slopeScaleDepthBias;
			}
			set
			{
				ThrowIfLocked();
				m_slopeScaleDepthBias = value;
			}
		}
	}
}
