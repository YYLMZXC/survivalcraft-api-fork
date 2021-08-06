using Engine.Serialization;
using Engine.Graphics;
using Engine;
namespace Game
{
    public class Subtexture : Engine.Serialization.Subtexture
    {
        public Subtexture(Texture2D texture, Vector2 topLeft, Vector2 bottomRight) : base(texture, topLeft, bottomRight)
        {
        }
    }
}
