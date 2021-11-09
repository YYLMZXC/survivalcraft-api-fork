using System.Collections.Generic;

namespace Engine.Graphics
{
	public class SkyShader : Shader
	{
		public ShaderParameter m_worldViewProjectionMatrixParameter;

		public ShaderParameter m_textureParameter;

		public ShaderParameter m_samplerStateParameter;

		public ShaderParameter m_colorParameter;

		public ShaderParameter m_alphaThresholdParameter;

		public readonly ShaderTransforms Transforms;

		public Texture2D Texture
		{
			set
			{
				m_textureParameter.SetValue(value);
			}
		}

		public SamplerState SamplerState
		{
			set
			{
				m_samplerStateParameter.SetValue(value);
			}
		}

		public Vector4 Color
		{
			set
			{
				m_colorParameter.SetValue(value);
			}
		}

		public float AlphaThreshold
		{
			set
			{
				m_alphaThresholdParameter.SetValue(value);
			}
		}

		public SkyShader(string vsc, string psc, bool useVertexColor, bool useTexture, bool useAlphaThreshold)
			: base(vsc, psc, PrepareShaderMacros(useVertexColor, useTexture, useAlphaThreshold))
		{
			m_worldViewProjectionMatrixParameter = base.GetParameter("u_worldViewProjectionMatrix", true);
			m_textureParameter = base.GetParameter("u_texture", true);
			m_samplerStateParameter = base.GetParameter("u_samplerState", true);
			m_colorParameter = base.GetParameter("u_color", true);
			m_alphaThresholdParameter = base.GetParameter("u_alphaThreshold", true);
			Transforms = new ShaderTransforms(1);
			Color = Vector4.One;
		}

		protected override void PrepareForDrawingOverride()
		{
			Transforms.UpdateMatrices(1, false, false, true);
			m_worldViewProjectionMatrixParameter.SetValue(this.Transforms.WorldViewProjection, 1);

		}

		public static ShaderMacro[] PrepareShaderMacros(bool useVertexColor, bool useTexture, bool useAlphaThreshold)
		{
			List<ShaderMacro> list = new List<ShaderMacro>();
			if (useVertexColor)
			{
				list.Add(new ShaderMacro("USE_VERTEXCOLOR"));
			}
			if (useTexture)
			{
				list.Add(new ShaderMacro("USE_TEXTURE"));
			}
			if (useAlphaThreshold)
			{
				list.Add(new ShaderMacro("USE_ALPHATHRESHOLD"));
			}
			return list.ToArray();
		}
	}
}
