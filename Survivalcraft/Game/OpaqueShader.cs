using Engine.Graphics;
using Engine;
namespace Game
{
   public class OpaqueShader:Shader
    {
        public OpaqueShader() : base(ModsManager.GetInPakOrStorageFile("Shaders/OpaqueVsh"), ModsManager.GetInPakOrStorageFile("Shaders/OpaquePsh"), new ShaderMacro[] { new ShaderMacro("OpaqueShader") }) { }

    }
}
