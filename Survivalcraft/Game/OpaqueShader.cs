using Engine.Graphics;
using Engine;
namespace Game
{
   public class OpaqueShader:Shader
    {
        public OpaqueShader() : base(ModsManager.GetInPakOrStorageFile("Shaders/Opaque",".vsh"), ModsManager.GetInPakOrStorageFile("Shaders/Opaque",".psh"), new ShaderMacro[] { new ShaderMacro("OpaqueShader") }) { }

    }
}
