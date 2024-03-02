namespace Engine.Handlers;

public interface ILitShaderHandler
{
    string VertexShaderCode { get; }
    string PixelsShaderCode { get; }
}