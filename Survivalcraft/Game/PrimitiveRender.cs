using Engine;
using Engine.Graphics;

namespace Game
{
	public class PrimitiveRender
	{
		public UnlitShader m_shader = new UnlitShader(ShaderCode.Get("Shaders/Unlit.vsh"), ShaderCode.Get("Shaders/Unlit.psh"), useVertexColor: true, useTexture: true, useAlphaThreshold: false);

		public UnlitShader m_shaderAlphaTest = new UnlitShader(ShaderCode.Get("Shaders/Unlit.vsh"), ShaderCode.Get("Shaders/Unlit.psh"), useVertexColor: true, useTexture: true, useAlphaThreshold: true);

		public void Textured_FlushWithCurrentStateAndShader(BaseTexturedBatch baseTexturedBatch, Shader shader, bool clearAfterFlush = true)
		{
			int num = 0;
			int num2 = baseTexturedBatch.TriangleIndices.Count;
			while (num2 > 0)
			{
				int num3 = MathUtils.Min(num2, 196605);
				Display.DrawUserIndexed(PrimitiveType.TriangleList, shader, VertexPositionColorTexture.VertexDeclaration, baseTexturedBatch.TriangleVertices.Array, 0, baseTexturedBatch.TriangleVertices.Count, baseTexturedBatch.TriangleIndices.Array, num, num3);
				num += num3;
				num2 -= num3;
			}
			if (clearAfterFlush)
			{
				baseTexturedBatch.Clear();
			}
		}

		public void Flush(PrimitivesRenderer3D primitiveRend, Matrix matrix, bool clearAfterFlush = true, int maxLayer = 2147483647)
		{
			if (primitiveRend.m_sortNeeded)
			{
				primitiveRend.m_sortNeeded = false;
				primitiveRend.m_allBatches.Sort(delegate (BaseBatch b1, BaseBatch b2)
				{
					if (b1.Layer < b2.Layer)
					{
						return -1;
					}
					if (b1.Layer <= b2.Layer)
					{
						return 0;
					}
					return 1;
				});
			}
			foreach (BaseBatch baseBatch in primitiveRend.m_allBatches)
			{
				if (baseBatch.Layer > maxLayer)
				{
					break;
				}
				if ((!baseBatch.IsEmpty()) && baseBatch is TexturedBatch3D)
				{

					BaseTexturedBatch baseTexturedBatch = (BaseTexturedBatch)baseBatch;
					Display.DepthStencilState = baseTexturedBatch.DepthStencilState;
					Display.RasterizerState = baseTexturedBatch.RasterizerState;
					Display.BlendState = baseTexturedBatch.BlendState;

					if (baseTexturedBatch.UseAlphaTest)
					{
						m_shaderAlphaTest.Texture = baseTexturedBatch.Texture;
						m_shaderAlphaTest.SamplerState = baseTexturedBatch.SamplerState;
						m_shaderAlphaTest.Transforms.World[0] = matrix;
						m_shaderAlphaTest.AlphaThreshold = 0f;
						baseTexturedBatch.FlushWithCurrentStateAndShader(m_shaderAlphaTest, clearAfterFlush);
					}
					else
					{
						m_shader.Texture = baseTexturedBatch.Texture;
						m_shader.SamplerState = baseTexturedBatch.SamplerState;
						m_shader.Transforms.World[0] = matrix;
						baseTexturedBatch.FlushWithCurrentStateAndShader(m_shader, clearAfterFlush);
					}
				}
				else
				{
					// I added support only for items(TexturedBatch3D), if not item, it will be rendered normally without rendered shaders
					baseBatch.Flush(matrix, clearAfterFlush: true);
				}
			}
		}
	}
}
