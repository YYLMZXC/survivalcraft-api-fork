namespace Engine.Graphics
{
	public abstract class BaseTexturedBatch : BaseBatch
	{
		public static UnlitShader Shader = new(useVertexColor: true, useTexture: true, useAlphaThreshold: false);

		public static UnlitShader ShaderAlphaTest = new(useVertexColor: true, useTexture: true, useAlphaThreshold: true);

		public readonly DynamicArray<VertexPositionColorTexture> TriangleVertices = [];

		public readonly DynamicArray<int> TriangleIndices = [];

		public Texture2D Texture
		{
			get;
			set;
		}

		public bool UseAlphaTest
		{
			get;
			set;
		}

		public SamplerState SamplerState
		{
			get;
			set;
		}

		internal BaseTexturedBatch()
		{
		}

		public override bool IsEmpty()
		{
			return TriangleIndices.Count == 0;
		}

		public override void Clear()
		{
			TriangleVertices.Clear();
			TriangleIndices.Clear();
		}

		public override void Flush(Matrix matrix, bool clearAfterFlush = true)
		{
			Display.DepthStencilState = base.DepthStencilState;
			Display.RasterizerState = base.RasterizerState;
			Display.BlendState = base.BlendState;
			FlushWithCurrentState(UseAlphaTest, Texture, SamplerState, matrix, clearAfterFlush);
		}

		public void FlushWithCurrentState(bool useAlphaTest, Texture2D texture, SamplerState samplerState, Matrix matrix, bool clearAfterFlush = true)
		{
			if (useAlphaTest)
			{
				ShaderAlphaTest.Texture = texture;
				ShaderAlphaTest.SamplerState = samplerState;
				ShaderAlphaTest.Transforms.World[0] = matrix;
				ShaderAlphaTest.AlphaThreshold = 0f;
				FlushWithCurrentStateAndShader(ShaderAlphaTest, clearAfterFlush);
			}
			else
			{
				Shader.Texture = texture;
				Shader.SamplerState = samplerState;
				Shader.Transforms.World[0] = matrix;
				FlushWithCurrentStateAndShader(Shader, clearAfterFlush);
			}
		}

		public void FlushWithCurrentStateAndShader(Shader shader, bool clearAfterFlush = true)
		{
			int num = 0;
			int num2 = TriangleIndices.Count;
			while (num2 > 0)
			{
				int num3 = MathUtils.Min(num2, 196605);
				Display.DrawUserIndexed(PrimitiveType.TriangleList, shader, VertexPositionColorTexture.VertexDeclaration, TriangleVertices.Array, 0, TriangleVertices.Count, TriangleIndices.Array, num, num3);
				num += num3;
				num2 -= num3;
			}
			if (clearAfterFlush)
			{
				Clear();
			}
		}
	}
}
