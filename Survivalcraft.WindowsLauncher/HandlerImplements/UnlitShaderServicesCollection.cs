using Engine.Graphics;

namespace Engine.Handlers;

public class UnlitShaderServicesCollection : IUnlitShaderHandler
{
    public string PixelShaderCode { get; } =
        new StreamReader(typeof(Shader).Assembly.GetManifestResourceStream("Engine.Resources.Unlit.psh")!)
            .ReadToEnd();

    public string VertexShaderCode { get; } =
        new StreamReader(typeof(Shader).Assembly.GetManifestResourceStream("Engine.Resources.Unlit.vsh")!)
            .ReadToEnd();
}