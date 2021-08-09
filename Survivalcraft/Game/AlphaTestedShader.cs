using Engine.Graphics;

namespace Game
{
   public class AlphaTestedShader:Shader
    {
        public AlphaTestedShader() : base(ModsManager.GetInPakOrStorageFile("Shaders/AlphaTested",".vsh"), ModsManager.GetInPakOrStorageFile("Shaders/AlphaTested",".psh"), new ShaderMacro[] { new ShaderMacro("ALPHATESTED","0.5") }) { }
    }
}
