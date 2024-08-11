using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine;

namespace Game
{
    public interface IPostprocessExplosions
    {
        public void OnExplosion(SubsystemExplosions subsystemExplosions, Vector3 impulse, float damage, out bool skipVanilla);
    }
}
