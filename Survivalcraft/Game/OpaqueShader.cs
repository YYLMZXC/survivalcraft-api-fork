using Engine.Graphics;
namespace Game
{
   public class OpaqueShader:Shader
    {
        public OpaqueShader() : base(ContentManager.Get<string>("Shaders/OpaqueVsh"), ContentManager.Get<string>("Shaders/OpaquePsh"), new ShaderMacro[] { new ShaderMacro("OpaqueShader") }) { }

    }
}
