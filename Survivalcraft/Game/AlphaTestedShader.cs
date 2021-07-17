using Engine.Graphics;

namespace Game
{
   public class AlphaTestedShader:Shader
    {
        public AlphaTestedShader() : base(ModsManager.GetInPakOrStorageFile("Shaders/AlphaTestedVsh"), ModsManager.GetInPakOrStorageFile("Shaders/AlphaTestedPsh"), new ShaderMacro[] { new ShaderMacro("ALPHATESTED","0.5") }) { }
    }
}
