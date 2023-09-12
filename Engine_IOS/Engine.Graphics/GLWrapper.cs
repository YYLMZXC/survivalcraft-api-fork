using OpenTK.Graphics.ES20;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Engine.Graphics
{
    internal static class GLWrapper
    {
        public static int m_arrayBuffer;

        public static int m_elementArrayBuffer;

        public static int m_texture2D;

        public static int[] m_activeTexturesByUnit;

        public static TextureUnit? m_activeTextureUnit;

        public static int m_program;

        public static int m_framebuffer;

        public static Vector4? m_clearColor;

        public static float? m_clearDepth;

        public static int? m_clearStencil;

        public static CullFaceMode? m_cullFace;

        public static FrontFaceDirection? m_frontFace;

        public static DepthFunction? m_depthFunction;

        public static int? m_colorMask;

        public static bool? m_depthMask;

        public static float? m_polygonOffsetFactor;

        public static float? m_polygonOffsetUnits;

        public static Vector4? m_blendColor;

        public static BlendEquationMode? m_blendEquation;//??

        public static BlendEquationMode? m_blendEquationColor;//??

        public static BlendEquationMode? m_blendEquationAlpha;//??

        public static BlendingFactorSrc? m_blendFuncSource;

        public static BlendingFactorSrc? m_blendFuncSourceColor;

        public static BlendingFactorSrc? m_blendFuncSourceAlpha;

        public static BlendingFactorDest? m_blendFuncDestination;

        public static BlendingFactorDest? m_blendFuncDestinationColor;

        public static BlendingFactorDest? m_blendFuncDestinationAlpha;

        public static Dictionary<EnableCap, bool> m_enableDisableStates;

        public static bool?[] m_vertexAttribArray;

        public static RasterizerState m_rasterizerState;

        public static DepthStencilState m_depthStencilState;

        public static BlendState m_blendState;

        public static Dictionary<int, SamplerState> m_textureSamplerStates;

        public static Shader m_lastShader;

        public static VertexDeclaration m_lastVertexDeclaration;

        public static IntPtr m_lastVertexOffset;

        public static int m_lastArrayBuffer;

        public static Viewport? m_viewport;

        public static Rectangle? m_scissorRectangle;

        public static bool GL_EXT_texture_filter_anisotropic;

        public static bool GL_OES_packed_depth_stencil;

        public static void Initialize()
        {
            Log.Information("GLES Vendor: " + GL.GetString(StringName.Vendor));
            Log.Information("GLES Renderer: " + GL.GetString(StringName.Renderer));
            Log.Information("GLES Version: " + GL.GetString(StringName.Version));
            string @string = GL.GetString(StringName.Extensions);
            GL_EXT_texture_filter_anisotropic = @string.Contains("GL_EXT_texture_filter_anisotropic");
            GL_OES_packed_depth_stencil = @string.Contains("GL_OES_packed_depth_stencil");
            Log.Information("GLES Extensions: " + @string);
        }

        public static void InitializeCache()
        {
            m_arrayBuffer = -1;
            m_elementArrayBuffer = -1;
            m_texture2D = -1;
            m_activeTexturesByUnit = new int[8]
            {
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1
            };
            m_activeTextureUnit = null;
            m_program = -1;
            m_framebuffer = -1;
            m_clearColor = null;
            m_clearDepth = null;
            m_clearStencil = null;
            m_cullFace = null;
            m_frontFace = null;
            m_depthFunction = null;
            m_colorMask = null;
            m_depthMask = null;
            m_polygonOffsetFactor = 0;
            m_polygonOffsetUnits = 0;
            m_blendColor = new Vector4(float.MinValue);
            m_blendEquation = null;
            m_blendEquationColor = null;
            m_blendEquationAlpha = null;
            m_blendFuncSource = null;
            m_blendFuncSourceColor = null;
            m_blendFuncSourceAlpha = null;
            m_blendFuncDestination = null;
            m_blendFuncDestinationColor = null;
            m_blendFuncDestinationAlpha = null;
            m_enableDisableStates = new Dictionary<EnableCap, bool>();
            m_vertexAttribArray = new bool?[16];
            m_rasterizerState = null;
            m_depthStencilState = null;
            m_blendState = null;
            m_textureSamplerStates = new Dictionary<int, SamplerState>();
            m_lastShader = null;
            m_lastVertexDeclaration = null;
            m_lastVertexOffset = IntPtr.Zero;
            m_lastArrayBuffer = -1;
            m_viewport = null;
            m_scissorRectangle = null;
        }


        public static bool Enable(EnableCap state)
        {
            if (!m_enableDisableStates.ContainsKey(state))
            {
                m_enableDisableStates[state] = false;
            }
            if (!m_enableDisableStates[state])
            {
                GL.Enable(state);
                m_enableDisableStates[state] = true;
                return true;
            }
            return false;
        }


        public static bool Disable(EnableCap state)
        {
            if (!m_enableDisableStates.ContainsKey(state))
            {
                m_enableDisableStates[state] = true;
            }
            if (m_enableDisableStates[state])
            {
                GL.Disable(state);
                m_enableDisableStates[state] = false;
                return true;
            }
            return false;
        }


        public static bool IsEnabled(EnableCap state)
        {
            if (!m_enableDisableStates.TryGetValue(state, out bool value))
            {
                value = GL.IsEnabled(state);
                m_enableDisableStates[state] = value;
            }
            return value;
        }

        public static void ClearColor(Vector4 color)
        {
            if (color != m_clearColor)
            {
                GL.ClearColor(color.X, color.Y, color.Z, color.W);
                m_clearColor = color;
            }
        }

        public static void ClearDepth(float depth)
        {
            if (depth != m_clearDepth)
            {
                GL.ClearDepth(depth);
                m_clearDepth = depth;
            }
        }

        public static void ClearStencil(int stencil)
        {
            if (stencil != m_clearStencil)
            {
                GL.ClearStencil(stencil);
                m_clearStencil = stencil;
            }
        }


        public static void CullFace(CullFaceMode cullFace)
        {
            if (cullFace != m_cullFace)
            {
                GL.CullFace(cullFace);
                m_cullFace = cullFace;
            }
        }


        public static void FrontFace(FrontFaceDirection frontFace)
        {
            if (frontFace != m_frontFace)
            {
                GL.FrontFace(frontFace);
                m_frontFace = frontFace;
            }
        }


        public static void DepthFunc(DepthFunction depthFunction)
        {
            if (depthFunction != m_depthFunction)
            {
                GL.DepthFunc(depthFunction);
                m_depthFunction = depthFunction;
            }
        }

        public static void ColorMask(int colorMask)
        {
            colorMask &= 0xF;
            if (colorMask != m_colorMask)
            {
                GL.ColorMask((colorMask & 8) != 0, (colorMask & 4) != 0, (colorMask & 2) != 0, (colorMask & 1) != 0);
                m_colorMask = colorMask;
            }
        }

        public static bool DepthMask(bool depthMask)
        {
            if (depthMask != m_depthMask)
            {
                GL.DepthMask(depthMask);
                m_depthMask = depthMask;
                return true;
            }
            return false;
        }

        public static void PolygonOffset(float factor, float units)
        {
            if (factor != m_polygonOffsetFactor || units != m_polygonOffsetUnits)
            {
                GL.PolygonOffset(factor, units);
                m_polygonOffsetFactor = factor;
                m_polygonOffsetUnits = units;
            }
        }

        public static void BlendColor(Vector4 blendColor)
        {
            if (blendColor != m_blendColor)
            {
                GL.BlendColor(blendColor.X, blendColor.Y, blendColor.Z, blendColor.W);
                m_blendColor = blendColor;
            }
        }


        public static void BlendEquation(BlendEquationMode blendEquation)
        {
            if (blendEquation != m_blendEquation)
            {
                m_blendEquation = blendEquation;
                m_blendEquationColor = null;
                m_blendEquationAlpha = null;
            }
        }


        public static void BlendEquationSeparate(BlendEquationMode blendEquationColor, BlendEquationMode blendEquationAlpha)
        {
            if (blendEquationColor != m_blendEquationColor || blendEquationAlpha != m_blendEquationAlpha)
            {
                GL.BlendEquationSeparate(blendEquationColor, blendEquationAlpha);
                m_blendEquationColor = blendEquationColor;
                m_blendEquationAlpha = blendEquationAlpha;
                m_blendEquation = null;
            }
        }


        public static void BlendFunc(BlendingFactorSrc blendFuncSource, BlendingFactorDest blendFuncDestination)
        {
            if (blendFuncSource != m_blendFuncSource || blendFuncDestination != m_blendFuncDestination)
            {
                GL.BlendFunc(blendFuncSource, blendFuncDestination);
                m_blendFuncSource = blendFuncSource;
                m_blendFuncDestination = blendFuncDestination;
                m_blendFuncSourceColor = null;
                m_blendFuncSourceAlpha = null;
                m_blendFuncDestinationColor = null;
                m_blendFuncDestinationAlpha = null;
            }
        }


        public static void BlendFuncSeparate(BlendingFactorSrc blendFuncSourceColor, BlendingFactorDest blendFuncDestinationColor, BlendingFactorSrc blendFuncSourceAlpha, BlendingFactorDest blendFuncDestinationAlpha)
        {
            if (blendFuncSourceColor != m_blendFuncSourceColor || blendFuncDestinationColor != m_blendFuncDestinationColor || blendFuncSourceAlpha != m_blendFuncSourceAlpha || blendFuncDestinationAlpha != m_blendFuncDestinationAlpha)
            {
                GL.BlendFuncSeparate(blendFuncSourceColor, blendFuncDestinationColor, blendFuncSourceAlpha, blendFuncDestinationAlpha);
                m_blendFuncSourceColor = blendFuncSourceColor;
                m_blendFuncSourceAlpha = blendFuncSourceAlpha;
                m_blendFuncDestinationColor = blendFuncDestinationColor;
                m_blendFuncDestinationAlpha = blendFuncDestinationAlpha;
                m_blendFuncSource = null;
                m_blendFuncDestination = null;
            }
        }

        public static void VertexAttribArray(int index, bool enable)
        {
            if (enable && (!m_vertexAttribArray[index].HasValue || !m_vertexAttribArray[index].Value))
            {
                GL.EnableVertexAttribArray(index);
                m_vertexAttribArray[index] = true;
            }
            else if (!enable && (!m_vertexAttribArray[index].HasValue || m_vertexAttribArray[index].Value))
            {
                GL.DisableVertexAttribArray(index);
                m_vertexAttribArray[index] = false;
            }
        }


        public static void BindTexture(TextureTarget target, int texture, bool forceBind)
        {
            if (target == TextureTarget.Texture2D)
            {
                if (forceBind || texture != m_texture2D)
                {
                    GL.BindTexture(target, texture);
                    m_texture2D = texture;
                    if (m_activeTextureUnit >= TextureUnit.Texture0)
                    {
                        m_activeTexturesByUnit[m_activeTextureUnit.Value - TextureUnit.Texture0] = texture;
                    }
                }
            }
            else
            {
                GL.BindTexture(target, texture);
            }
        }


        public static void ActiveTexture(TextureUnit textureUnit)
        {
            if (textureUnit != m_activeTextureUnit)
            {
                GL.ActiveTexture(textureUnit);
                m_activeTextureUnit = textureUnit;
            }
        }


        public static void BindBuffer(BufferTarget target, int buffer)
        {
            switch (target)
            {
                case BufferTarget.ArrayBuffer:
                    if (buffer != m_arrayBuffer)
                    {
                        GL.BindBuffer(target, buffer);
                        m_arrayBuffer = buffer;
                    }
                    break;
                case BufferTarget.ElementArrayBuffer:
                    if (buffer != m_elementArrayBuffer)
                    {
                        GL.BindBuffer(target, buffer);
                        m_elementArrayBuffer = buffer;
                    }
                    break;
                default:
                    GL.BindBuffer(target, buffer);
                    break;
            }
        }

        public static void BindFramebuffer(int framebuffer)
        {
            if (framebuffer != m_framebuffer)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
                m_framebuffer = framebuffer;
            }
        }

        public static void UseProgram(int program)
        {
            if (program != m_program)
            {
                GL.UseProgram(program);
                m_program = program;
            }
        }

        public static void DeleteProgram(int program)
        {
            if (m_program == program)
            {
                m_program = -1;
            }
            GL.DeleteProgram(program);
        }

        public static void DeleteTexture(int texture)
        {
            if (m_texture2D == texture)
            {
                m_texture2D = -1;
            }
            for (int i = 0; i < m_activeTexturesByUnit.Length; i++)
            {
                if (m_activeTexturesByUnit[i] == texture)
                {
                    m_activeTexturesByUnit[i] = -1;
                }
            }
            m_textureSamplerStates.Remove(texture);
            GL.DeleteTexture(texture);
        }

        public static void DeleteFramebuffer(int framebuffer)
        {
            if (m_framebuffer == framebuffer)
            {
                m_framebuffer = -1;
            }
            GL.DeleteFramebuffers(1, ref framebuffer);
        }


        public static void DeleteBuffer(All target, int buffer)
        {
            if (target == All.ArrayBuffer)
            {
                if (m_arrayBuffer == buffer)
                {
                    m_arrayBuffer = -1;
                }
                if (m_lastArrayBuffer == buffer)
                {
                    m_lastArrayBuffer = -1;
                }
            }
            if (target == All.ElementArrayBuffer && m_elementArrayBuffer == buffer)
            {
                m_elementArrayBuffer = -1;
            }
            GL.DeleteBuffers(1, ref buffer);
        }

        public static void ApplyViewportScissor(Viewport viewport, Rectangle scissorRectangle, bool isScissorEnabled)
        {
            if (!m_viewport.HasValue || viewport.X != m_viewport.Value.X || viewport.Y != m_viewport.Value.Y || viewport.Width != m_viewport.Value.Width || viewport.Height != m_viewport.Value.Height)
            {
                int y = (Display.RenderTarget == null) ? (Display.BackbufferSize.Y - viewport.Y - viewport.Height) : viewport.Y;
                GL.Viewport(viewport.X, y, viewport.Width, viewport.Height);
            }
            if (!m_viewport.HasValue || viewport.MinDepth != m_viewport.Value.MinDepth || viewport.MaxDepth != m_viewport.Value.MaxDepth)
            {
                GL.DepthRange(viewport.MinDepth, viewport.MaxDepth);
            }
            m_viewport = viewport;
            if (!isScissorEnabled)
            {
                return;
            }
            if (scissorRectangle == m_scissorRectangle)
            {
                return;
            }
            if (Display.RenderTarget == null)
            {
                scissorRectangle.Top = Display.BackbufferSize.Y - scissorRectangle.Top - scissorRectangle.Height;
            }
            GL.Scissor(scissorRectangle.Left, scissorRectangle.Top, scissorRectangle.Width, scissorRectangle.Height);
            m_scissorRectangle = scissorRectangle;
        }

        public static void ApplyRasterizerState(RasterizerState state)
        {
            if (state != m_rasterizerState)
            {
                m_rasterizerState = state;
                switch (state.CullMode)
                {
                    case CullMode.None:
                        Disable(EnableCap.CullFace);
                        break;
                    case CullMode.CullClockwise:
                        Enable(EnableCap.CullFace);
                        CullFace(CullFaceMode.Back);
                        FrontFace((Display.RenderTarget != null) ? FrontFaceDirection.Cw : FrontFaceDirection.Ccw);
                        break;
                    case CullMode.CullCounterClockwise:
                        Enable(EnableCap.CullFace);
                        CullFace(CullFaceMode.Back);
                        FrontFace((Display.RenderTarget != null) ? FrontFaceDirection.Ccw : FrontFaceDirection.Cw);
                        break;
                }
                if (state.ScissorTestEnable)
                {
                    Enable(EnableCap.ScissorTest);
                }
                else
                {
                    Disable(EnableCap.ScissorTest);
                }
                if (state.DepthBias != 0f || state.SlopeScaleDepthBias != 0f)
                {
                    Enable(EnableCap.PolygonOffsetFill);
                    PolygonOffset(state.SlopeScaleDepthBias, state.DepthBias);
                }
                else
                {
                    Disable(EnableCap.PolygonOffsetFill);
                }
            }
        }

        public static void ApplyDepthStencilState(DepthStencilState state)
        {
            if (state != m_depthStencilState)
            {
                m_depthStencilState = state;
                if (state.DepthBufferTestEnable || state.DepthBufferWriteEnable)
                {
                    Enable(EnableCap.DepthTest);
                    if (state.DepthBufferTestEnable)
                    {
                        DepthFunc(TranslateCompareFunction(state.DepthBufferFunction));
                    }
                    else
                    {
                        DepthFunc(DepthFunction.Always);
                    }
                    DepthMask(state.DepthBufferWriteEnable);
                }
                else
                {
                    Disable(EnableCap.DepthTest);
                }
            }
        }

        public static void ApplyBlendState(BlendState state)
        {
            if (state == m_blendState)
            {
                return;
            }
            m_blendState = state;
            if (state.ColorBlendFunction == BlendFunction.Add && state.ColorSourceBlend == Blend.One && state.ColorDestinationBlend == Blend.Zero && state.AlphaBlendFunction == BlendFunction.Add && state.AlphaSourceBlend == Blend.One && state.AlphaDestinationBlend == Blend.Zero)
            {
                Disable(EnableCap.Blend);
                return;
            }
            BlendEquationMode all = TranslateBlendFunction(state.ColorBlendFunction);
            BlendEquationMode all2 = TranslateBlendFunction(state.AlphaBlendFunction);
            BlendingFactorSrc all3 = TranslateBlendSrc(state.ColorSourceBlend);
            BlendingFactorDest all4 = TranslateBlendDest(state.ColorDestinationBlend);
            BlendingFactorSrc all5 = TranslateBlendSrc(state.AlphaSourceBlend);
            BlendingFactorDest all6 = TranslateBlendDest(state.AlphaDestinationBlend);
            if (all == all2 && all3 == all5 && all4 == all6)
            {
                BlendEquation(all);
                BlendFunc(all3, all4);
            }
            else
            {
                BlendEquationSeparate(all, all2);
                BlendFuncSeparate(all3, all4, all5, all6);
            }
            BlendColor(state.BlendFactor);
            Enable(EnableCap.Blend);
        }

        public static void ApplyRenderTarget(RenderTarget2D renderTarget)
        {
            if (renderTarget != null)
            {
                BindFramebuffer(renderTarget.m_frameBuffer);
            }
            else
            {
                BindFramebuffer(1);
            }
        }

        public static void ApplyShaderAndBuffers(Shader shader, VertexDeclaration vertexDeclaration, IntPtr vertexOffset, int arrayBuffer, int? elementArrayBuffer)
        {
            shader.PrepareForDrawing();
            BindBuffer(BufferTarget.ArrayBuffer, arrayBuffer);
            if (elementArrayBuffer.HasValue)
            {
                BindBuffer(BufferTarget.ElementArrayBuffer, elementArrayBuffer.Value);
            }
            UseProgram(shader.m_program);
            if (shader != m_lastShader || vertexOffset != m_lastVertexOffset || arrayBuffer != m_lastArrayBuffer || vertexDeclaration.m_elements != m_lastVertexDeclaration.m_elements)
            {
                Shader.VertexAttributeData[] vertexAttribData = shader.GetVertexAttribData(vertexDeclaration);
                for (int i = 0; i < vertexAttribData.Length; i++)
                {
                    if (vertexAttribData[i].Size != 0)
                    {
                        GL.VertexAttribPointer(i, vertexAttribData[i].Size, vertexAttribData[i].Type, vertexAttribData[i].Normalize, vertexDeclaration.VertexStride, vertexOffset + vertexAttribData[i].Offset);
                        VertexAttribArray(i, enable: true);
                    }
                    else
                    {
                        VertexAttribArray(i, enable: false);
                    }
                }
                m_lastShader = shader;
                m_lastVertexDeclaration = vertexDeclaration;
                m_lastVertexOffset = vertexOffset;
                m_lastArrayBuffer = arrayBuffer;
            }
            int num = 0;
            int num2 = 0;
            ShaderParameter shaderParameter;
            while (true)
            {
                if (num2 >= shader.m_parameters.Length)
                {
                    return;
                }
                shaderParameter = shader.m_parameters[num2];
                if (shaderParameter.IsChanged)
                {
                    switch (shaderParameter.Type)
                    {
                        case ShaderParameterType.Float:
                            GL.Uniform1(shaderParameter.Location, shaderParameter.Count, shaderParameter.Value);
                            shaderParameter.IsChanged = false;
                            break;
                        case ShaderParameterType.Vector2:
                            GL.Uniform2(shaderParameter.Location, shaderParameter.Count, shaderParameter.Value);
                            shaderParameter.IsChanged = false;
                            break;
                        case ShaderParameterType.Vector3:
                            GL.Uniform3(shaderParameter.Location, shaderParameter.Count, shaderParameter.Value);
                            shaderParameter.IsChanged = false;
                            break;
                        case ShaderParameterType.Vector4:
                            GL.Uniform4(shaderParameter.Location, shaderParameter.Count, shaderParameter.Value);
                            shaderParameter.IsChanged = false;
                            break;
                        case ShaderParameterType.Matrix:
                            GL.UniformMatrix4(shaderParameter.Location, shaderParameter.Count, transpose: false, shaderParameter.Value);
                            shaderParameter.IsChanged = false;
                            break;
                        default:
                            throw new InvalidOperationException("Unsupported shader parameter type.");
                        case ShaderParameterType.Texture2D:
                        case ShaderParameterType.Sampler2D:
                            break;
                    }
                }
                if (shaderParameter.Type == ShaderParameterType.Texture2D)
                {
                    if (num >= 8)
                    {
                        throw new InvalidOperationException("Too many simultaneous textures.");
                    }
                    ActiveTexture(TextureUnit.Texture0 + num);
                    if (shaderParameter.IsChanged)
                    {
                        GL.Uniform1(shaderParameter.Location, num);
                    }
                    ShaderParameter obj = shader.m_parameters[num2 + 1];
                    Texture2D texture2D = (Texture2D)shaderParameter.Resource;
                    SamplerState samplerState = (SamplerState)obj.Resource;
                    if (texture2D != null)
                    {
                        if (samplerState == null)
                        {
                            break;
                        }
                        if (m_activeTexturesByUnit[num] != texture2D.m_texture)
                        {
                            BindTexture(TextureTarget.Texture2D, texture2D.m_texture, forceBind: true);
                        }
                        if (!m_textureSamplerStates.TryGetValue(texture2D.m_texture, out SamplerState value) || value != samplerState)
                        {
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TranslateTextureFilterModeMin(samplerState.FilterMode, texture2D.MipLevelsCount > 1));
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TranslateTextureFilterModeMag(samplerState.FilterMode));
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TranslateTextureAddressMode(samplerState.AddressModeU));
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TranslateTextureAddressMode(samplerState.AddressModeV));
                            m_textureSamplerStates[texture2D.m_texture] = samplerState;
                        }
                    }
                    else if (m_activeTexturesByUnit[num] != 0)
                    {
                        BindTexture(TextureTarget.Texture2D, 0, forceBind: true);
                    }
                    num++;
                    shaderParameter.IsChanged = false;
                }
                num2++;
            }
            throw new InvalidOperationException($"Associated SamplerState is not set for texture \"{shaderParameter.Name}\".");
        }

        public static void Clear(RenderTarget2D renderTarget, Vector4? color, float? depth, int? stencil)
        {
            ClearBufferMask all = default;
            if (color.HasValue)
            {
                all |= ClearBufferMask.ColorBufferBit;
                ClearColor(color.Value);
                ColorMask(15);
            }
            if (depth.HasValue)
            {
                all |= ClearBufferMask.DepthBufferBit;
                ClearDepth(depth.Value);
                if (DepthMask(depthMask: true))
                {
                    m_depthStencilState = null;
                }
            }
            if (stencil.HasValue)
            {
                all |= ClearBufferMask.StencilBufferBit;
                ClearStencil(stencil.Value);
            }
            if (color.HasValue || depth.HasValue || stencil.HasValue)
            {
                ApplyRenderTarget(renderTarget);
                if (Disable(EnableCap.ScissorTest))
                {
                    m_rasterizerState = null;
                }
                GL.Clear(all);
            }
        }

        public static void HandleContextLost()
        {
            try
            {
                Log.Information("Device lost");
                Display.HandleDeviceLost();
                GC.Collect();
                InitializeCache();
                Display.Resize();
                Display.HandleDeviceReset();
                Log.Information("Device reset");
            }
            catch (Exception ex)
            {
                Log.Error("Failed to recreate graphics resources. Reason: {0}", ex.Message);
            }
        }


        public static void TranslateVertexElementFormat(VertexElementFormat vertexElementFormat, out All type, out bool normalize)
        {
            switch (vertexElementFormat)
            {
                case VertexElementFormat.Single:
                    type = All.Float;
                    normalize = false;
                    break;
                case VertexElementFormat.Vector2:
                    type = All.Float;
                    normalize = false;
                    break;
                case VertexElementFormat.Vector3:
                    type = All.Float;
                    normalize = false;
                    break;
                case VertexElementFormat.Vector4:
                    type = All.Float;
                    normalize = false;
                    break;
                case VertexElementFormat.Byte4:
                    type = All.UnsignedByte;
                    normalize = false;
                    break;
                case VertexElementFormat.NormalizedByte4:
                    type = All.UnsignedByte;
                    normalize = true;
                    break;
                case VertexElementFormat.Short2:
                    type = All.Short;
                    normalize = false;
                    break;
                case VertexElementFormat.NormalizedShort2:
                    type = All.Short;
                    normalize = true;
                    break;
                case VertexElementFormat.Short4:
                    type = All.Short;
                    normalize = false;
                    break;
                case VertexElementFormat.NormalizedShort4:
                    type = All.Short;
                    normalize = true;
                    break;
                default:
                    throw new InvalidOperationException("Unsupported vertex element format.");
            }
        }


        public static DrawElementsType TranslateIndexFormat(IndexFormat indexFormat)
        {
            switch (indexFormat)
            {
                case IndexFormat.SixteenBits:
                    return DrawElementsType.UnsignedShort;
                case IndexFormat.ThirtyTwoBits:
                    return DrawElementsType.UnsignedInt;
                default:
                    throw new InvalidOperationException("Unsupported index format.");
            }
        }


        public static ShaderParameterType TranslateActiveUniformType(ActiveUniformType type)
        {
            switch (type)
            {
                case ActiveUniformType.Float:
                    return ShaderParameterType.Float;
                case ActiveUniformType.FloatVec2:
                    return ShaderParameterType.Vector2;
                case ActiveUniformType.FloatVec3:
                    return ShaderParameterType.Vector3;
                case ActiveUniformType.FloatVec4:
                    return ShaderParameterType.Vector4;
                case ActiveUniformType.FloatMat4:
                    return ShaderParameterType.Matrix;
                case ActiveUniformType.Sampler2D:
                    return ShaderParameterType.Texture2D;
                default:
                    throw new InvalidOperationException("Unsupported shader parameter type.");
            }
        }


        public static BeginMode TranslatePrimitiveType(PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.LineList:
                    return BeginMode.Lines;
                case PrimitiveType.LineStrip:
                    return BeginMode.LineStrip;
                case PrimitiveType.TriangleList:
                    return BeginMode.Triangles;
                case PrimitiveType.TriangleStrip:
                    return BeginMode.TriangleStrip;
                default:
                    throw new InvalidOperationException("Unsupported primitive type.");
            }
        }


        public static All TranslateTextureFilterModeMin(TextureFilterMode filterMode, bool isMipmapped)
        {
            switch (filterMode)
            {
                case TextureFilterMode.Point:
                    if (!isMipmapped)
                    {
                        return All.Nearest;
                    }
                    return All.NearestMipmapNearest;
                case TextureFilterMode.Linear:
                    if (!isMipmapped)
                    {
                        return All.Linear;
                    }
                    return All.LinearMipmapLinear;
                case TextureFilterMode.Anisotropic:
                    if (!isMipmapped)
                    {
                        return All.Linear;
                    }
                    return All.LinearMipmapLinear;
                case TextureFilterMode.PointMipLinear:
                    if (!isMipmapped)
                    {
                        return All.Nearest;
                    }
                    return All.NearestMipmapLinear;
                case TextureFilterMode.LinearMipPoint:
                    if (!isMipmapped)
                    {
                        return All.Linear;
                    }
                    return All.LinearMipmapNearest;
                case TextureFilterMode.MinPointMagLinearMipPoint:
                    if (!isMipmapped)
                    {
                        return All.Nearest;
                    }
                    return All.NearestMipmapNearest;
                case TextureFilterMode.MinPointMagLinearMipLinear:
                    if (!isMipmapped)
                    {
                        return All.Nearest;
                    }
                    return All.NearestMipmapLinear;
                case TextureFilterMode.MinLinearMagPointMipPoint:
                    if (!isMipmapped)
                    {
                        return All.Linear;
                    }
                    return All.LinearMipmapNearest;
                case TextureFilterMode.MinLinearMagPointMipLinear:
                    if (!isMipmapped)
                    {
                        return All.Linear;
                    }
                    return All.LinearMipmapLinear;
                default:
                    throw new InvalidOperationException("Unsupported texture filter mode.");
            }
        }


        public static All TranslateTextureFilterModeMag(TextureFilterMode filterMode)
        {
            switch (filterMode)
            {
                case TextureFilterMode.Point:
                    return All.Nearest;
                case TextureFilterMode.Linear:
                    return All.Linear;
                case TextureFilterMode.Anisotropic:
                    return All.Linear;
                case TextureFilterMode.PointMipLinear:
                    return All.Nearest;
                case TextureFilterMode.LinearMipPoint:
                    return All.Nearest;
                case TextureFilterMode.MinPointMagLinearMipPoint:
                    return All.Linear;
                case TextureFilterMode.MinPointMagLinearMipLinear:
                    return All.Linear;
                case TextureFilterMode.MinLinearMagPointMipPoint:
                    return All.Nearest;
                case TextureFilterMode.MinLinearMagPointMipLinear:
                    return All.Nearest;
                default:
                    throw new InvalidOperationException("Unsupported texture filter mode.");
            }
        }


        public static All TranslateTextureAddressMode(TextureAddressMode addressMode)
        {
            switch (addressMode)
            {
                case TextureAddressMode.Clamp:
                    return All.ClampToEdge;
                case TextureAddressMode.Wrap:
                    return All.Repeat;
                default:
                    throw new InvalidOperationException("Unsupported texture address mode.");
            }
        }


        public static DepthFunction TranslateCompareFunction(CompareFunction compareFunction)
        {
            switch (compareFunction)
            {
                case CompareFunction.Always:
                    return DepthFunction.Always;
                case CompareFunction.Equal:
                    return DepthFunction.Equal;
                case CompareFunction.Greater:
                    return DepthFunction.Greater;
                case CompareFunction.GreaterEqual:
                    return DepthFunction.Gequal;
                case CompareFunction.Less:
                    return DepthFunction.Less;
                case CompareFunction.LessEqual:
                    return DepthFunction.Lequal;
                case CompareFunction.Never:
                    return DepthFunction.Never;
                case CompareFunction.NotEqual:
                    return DepthFunction.Notequal;
                default:
                    throw new InvalidOperationException("Unsupported texture address mode.");
            }
        }


        public static BlendEquationMode TranslateBlendFunction(BlendFunction blendFunction)
        {
            switch (blendFunction)
            {
                case BlendFunction.Add:
                    return BlendEquationMode.FuncAdd;
                case BlendFunction.Subtract:
                    return BlendEquationMode.FuncSubtract;
                case BlendFunction.ReverseSubtract:
                    return BlendEquationMode.FuncReverseSubtract;
                default:
                    throw new InvalidOperationException("Unsupported blend function.");
            }
        }


        public static BlendingFactorSrc TranslateBlendSrc(Blend blend)
        {
            switch (blend)
            {
                case Blend.Zero:
                    return BlendingFactorSrc.Zero;
                case Blend.One:
                    return BlendingFactorSrc.One;
                case Blend.SourceColor:
                    return BlendingFactorSrc.SrcColor;
                case Blend.InverseSourceColor:
                    return BlendingFactorSrc.OneMinusSrcColor;
                case Blend.DestinationColor:
                    return BlendingFactorSrc.DstColor;
                case Blend.InverseDestinationColor:
                    return BlendingFactorSrc.OneMinusDstColor;
                case Blend.SourceAlpha:
                    return BlendingFactorSrc.SrcAlpha;
                case Blend.InverseSourceAlpha:
                    return BlendingFactorSrc.OneMinusSrcAlpha;
                case Blend.DestinationAlpha:
                    return BlendingFactorSrc.DstAlpha;
                case Blend.InverseDestinationAlpha:
                    return BlendingFactorSrc.OneMinusDstAlpha;
                case Blend.BlendFactor:
                    return BlendingFactorSrc.ConstantColor;
                case Blend.InverseBlendFactor:
                    return BlendingFactorSrc.OneMinusConstantColor;
                case Blend.SourceAlphaSaturation:
                    return BlendingFactorSrc.SrcAlphaSaturate;
                default:
                    throw new InvalidOperationException("Unsupported blend.");
            }
        }
        public static BlendingFactorDest TranslateBlendDest(Blend blend)
        {
            switch (blend)
            {
                case Blend.Zero:
                    return BlendingFactorDest.Zero;
                case Blend.One:
                    return BlendingFactorDest.One;
                case Blend.SourceColor:
                    return BlendingFactorDest.SrcColor;
                case Blend.InverseSourceColor:
                    return BlendingFactorDest.OneMinusSrcColor;
                case Blend.DestinationColor:
                    return BlendingFactorDest.DstColor;
                case Blend.InverseDestinationColor:
                    return BlendingFactorDest.OneMinusDstColor;
                case Blend.SourceAlpha:
                    return BlendingFactorDest.SrcAlpha;
                case Blend.InverseSourceAlpha:
                    return BlendingFactorDest.OneMinusSrcAlpha;
                case Blend.DestinationAlpha:
                    return BlendingFactorDest.DstAlpha;
                case Blend.InverseDestinationAlpha:
                    return BlendingFactorDest.OneMinusDstAlpha;
                case Blend.BlendFactor:
                    return BlendingFactorDest.ConstantColor;
                case Blend.InverseBlendFactor:
                    return BlendingFactorDest.OneMinusConstantColor;
                case Blend.SourceAlphaSaturation:
                    return BlendingFactorDest.SrcAlphaSaturate;
                default:
                    throw new InvalidOperationException("Unsupported blend.");
            }
        }

        public static RenderbufferInternalFormat TranslateDepthFormat(DepthFormat depthFormat)
        {
            switch (depthFormat)
            {
                case DepthFormat.Depth24Stencil8:
                case DepthFormat.Depth16:
                    return RenderbufferInternalFormat.DepthComponent16;
                default:
                    throw new InvalidOperationException("Unsupported DepthFormat.");
            }
        }

        [Conditional("DEBUG")]
        public static void CheckGLError()
        {
        }
    }
}