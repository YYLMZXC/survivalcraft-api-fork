using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface IPlayerControlMovement
    {
        public void OnPlayerControlSteed(ComponentPlayer componentPlayer, bool skippedByOtherMods, out bool skipVanilla)
        {
            skipVanilla = false;
        }
        public void OnPlayerControlBoat(ComponentPlayer componentPlayer, bool skippedByOtherMods, out bool skipVanilla) 
        {
            skipVanilla = false;
        }

        public void OnPlayerControlWalk(ComponentPlayer componentPlayer, bool skippedByOtherMods, out bool skipVanilla)
        {
            skipVanilla = false;
        }
    }
}
