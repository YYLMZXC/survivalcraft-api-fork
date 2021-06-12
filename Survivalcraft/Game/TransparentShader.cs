using Engine.Graphics;
namespace Game
{
    public class TransparentShader:Shader
    {
        public TransparentShader() : base(ContentManager.Get<string>("Shaders/TransparentVsh"), ContentManager.Get<string>("Shaders/TransparentPsh"),new ShaderMacro[] { new ShaderMacro("TransparentShader") }) { }
    }
}
