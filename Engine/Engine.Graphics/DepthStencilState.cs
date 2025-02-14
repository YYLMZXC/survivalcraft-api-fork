namespace Engine.Graphics
{
	public  class DepthStencilState : LockOnFirstUse
	{
		private bool m_depthBufferTestEnable = true;

		private bool m_depthBufferWriteEnable = true;

		private CompareFunction m_depthBufferFunction = CompareFunction.LessEqual;

		public static readonly DepthStencilState Default = new()
		{
			IsLocked = true
		};

		public static readonly DepthStencilState DepthRead = new()
		{
			DepthBufferWriteEnable = false,
			IsLocked = true
		};

		public static readonly DepthStencilState DepthWrite = new()
		{
			DepthBufferTestEnable = false,
			IsLocked = true
		};

		public static readonly DepthStencilState None = new()
		{
			DepthBufferTestEnable = false,
			DepthBufferWriteEnable = false,
			IsLocked = true
		};

		public bool DepthBufferTestEnable
		{
			get
			{
				return m_depthBufferTestEnable;
			}
			set
			{
				ThrowIfLocked();
				m_depthBufferTestEnable = value;
			}
		}

		public bool DepthBufferWriteEnable
		{
			get
			{
				return m_depthBufferWriteEnable;
			}
			set
			{
				ThrowIfLocked();
				m_depthBufferWriteEnable = value;
			}
		}

		public CompareFunction DepthBufferFunction
		{
			get
			{
				return m_depthBufferFunction;
			}
			set
			{
				ThrowIfLocked();
				m_depthBufferFunction = value;
			}
		}
	}
}
