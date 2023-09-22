namespace Engine.Graphics
{
	public sealed class RasterizerState : LockOnFirstUse
	{
		private CullMode m_cullMode = CullMode.CullCounterClockwise;

		private bool m_scissorTestEnable;

		private float m_depthBias;

		private float m_slopeScaleDepthBias;

		public static readonly RasterizerState CullNone = new RasterizerState
		{
			CullMode = CullMode.None,
			IsLocked = true
		};

		public static readonly RasterizerState CullNoneScissor = new RasterizerState
		{
			CullMode = CullMode.None,
			ScissorTestEnable = true,
			IsLocked = true
		};

		public static readonly RasterizerState CullClockwise = new RasterizerState
		{
			CullMode = CullMode.CullClockwise,
			IsLocked = true
		};

		public static readonly RasterizerState CullClockwiseScissor = new RasterizerState
		{
			CullMode = CullMode.CullClockwise,
			ScissorTestEnable = true,
			IsLocked = true
		};

		public static readonly RasterizerState CullCounterClockwise = new RasterizerState
		{
			CullMode = CullMode.CullCounterClockwise,
			IsLocked = true
		};

		public static readonly RasterizerState CullCounterClockwiseScissor = new RasterizerState
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
        /// <summary>
        /// 剪裁测试是OpenGL中一个用于控制渲染区域的功能。当剪裁测试被启用时，只有位于指定剪裁矩形内的像素才会被绘制，而在剪裁矩形外的像素将被忽略。这在一些情况下非常有用，比如创建镜头视野范围或者限制渲染到特定区域。
        /// </summary>
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
