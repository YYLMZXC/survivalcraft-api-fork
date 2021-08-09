using Engine.Graphics;
namespace Game
{
    public class TransparentShader:Shader
    {
        public TransparentShader() : base(ModsManager.GetInPakOrStorageFile("Shaders/Transparent",".vsh"), ModsManager.GetInPakOrStorageFile("Shaders/Transparent",".psh"),new ShaderMacro[] { new ShaderMacro("TransparentShader") }) { }
    }
}
