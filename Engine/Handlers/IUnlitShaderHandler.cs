namespace Engine.Handlers;

public interface IUnlitShaderHandler
{
    string PixelShaderCode { get; }
    string VertexShaderCode { get; }
}