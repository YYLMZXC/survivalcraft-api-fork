using Engine.Graphics;
using Engine.Handlers;

namespace Survivalcraft.WindowsLauncher.HandlerImplements;

public class LitShaderHandler : ILitShaderHandler
{
    public string VertexShaderCode =>
        new StreamReader(typeof(Shader).Assembly.GetManifestResourceStream("Engine.Resources.Lit.vsh")!).ReadToEnd();

    public string PixelsShaderCode =>
        new StreamReader(typeof(Shader).Assembly.GetManifestResourceStream("Engine.Resources.Lit.psh")!).ReadToEnd();
}