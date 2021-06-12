using Engine.Graphics;

namespace Game
{
   public class AlphaTestedShader:Shader
    {
        public AlphaTestedShader() : base(ContentManager.Get<string>("Shaders/AlphaTestedVsh"), ContentManager.Get<string>("Shaders/AlphaTestedPsh"), new ShaderMacro[] { new ShaderMacro("ALPHATESTED","0.5") }) { }
    }
}
