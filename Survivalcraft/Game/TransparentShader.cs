using Engine.Graphics;
namespace Game
{
    public class TransparentShader:Shader
    {
        public TransparentShader() : base(ModsManager.GetInPakOrStorageFile("Shaders/TransparentVsh"), ModsManager.GetInPakOrStorageFile("Shaders/TransparentPsh"),new ShaderMacro[] { new ShaderMacro("TransparentShader") }) { }
    }
}
