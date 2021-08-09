using Engine.Graphics;
namespace Game
{
    public class HighlightShader:Shader
    {
        public HighlightShader() : base(ModsManager.GetInPakOrStorageFile("Shaders/Highlight",".vsh"), ModsManager.GetInPakOrStorageFile("Shaders/Highlight",".psh"), new ShaderMacro[] { new ShaderMacro("HighlightShader") }) { }
    }
}
