using Engine.Graphics;
namespace Game
{
    public class HighlightShader:Shader
    {
        public HighlightShader() : base(ContentManager.Get<string>("Shaders/HighlightVsh"), ContentManager.Get<string>("Shaders/HighlightPsh"), new ShaderMacro[] { new ShaderMacro("HighlightShader") }) { }
    }
}
