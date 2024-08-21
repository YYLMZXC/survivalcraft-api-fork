using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface IPlayerControlInput
    {
        public void OnPlayerInputInteract(ComponentPlayer componentPlayer, ref bool playerOperated, ref double timeIntervalLastActionTime, bool skippedByOtherMods, out bool skipVanilla)
        {
            skipVanilla = false;
        }

        public void UpdatePlayerInputAim(ComponentPlayer componentPlayer, bool aiming, ref bool playerOperated, ref float timeIntervalAim, bool skippedByOtherMods, out bool skipVanilla)
        {
            skipVanilla = false;
        }

        //在玩家执行“攻击”动作时执行，比如恒泰左键放箭，工业左键点击船
        public void OnPlayerInputHit(ComponentPlayer componentPlayer, ref bool playerOperated, ref double timeIntervalHit, bool skippedByOtherMods, out bool skipVanilla)
        {
            skipVanilla = false;
        }

        public void UpdatePlayerInputDig(ComponentPlayer componentPlayer, bool digging, ref bool playerOperated, ref double timeIntervalDig, bool skippedByOtherMods, out bool skipVanilla)
        {
            skipVanilla = false; 
        }

        public void OnPlayerInputDrop(ComponentPlayer componentPlayer, bool skippedByOtherMods, out bool skipVanilla)
        {
            skipVanilla = false;
        }
    }
}
