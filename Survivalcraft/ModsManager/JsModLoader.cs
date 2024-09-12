using Engine;
using GameEntitySystem;
using Jint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class JsModLoader:ModLoader
    {
        public override void __ModInitialize()
        {
            Engine.Log.Information("JsModLoader");
        }
        public override void OnMinerDig(ComponentMiner miner, TerrainRaycastResult raycastResult, ref float DigProgress, out bool Digged)
        {
            Engine.Log.Information("JsModLoader" + DigProgress);
            float DigProgress1 = DigProgress;
            bool Digged1 = false;
            JsInterface.handlersDictionary["OnMinerDig"].ForEach(function => {
                Digged1|=JsInterface.Invoke(function,miner,raycastResult,DigProgress1).AsBoolean(); 
            });
            Digged = Digged1;
        }
        public override void OnMinerPlace(ComponentMiner miner, TerrainRaycastResult raycastResult, int x, int y, int z, int value, out bool Placed)
        {
            Placed = false;
        }
        public override bool OnPlayerSpawned(PlayerData.SpawnMode spawnMode, ComponentPlayer componentPlayer, Vector3 position)
        {
            return false;
        }
        public override void OnPlayerDead(PlayerData playerData)
        {
        }
        public override bool AttackBody(ComponentBody target, Entity attacker, Vector3 hitPoint, Vector3 hitDirection, ref float attackPower, bool isMeleeAttack)
        {
            return false;
        }
        public override void OnCreatureInjure(ComponentHealth componentHealth, float amount, ComponentCreature attacker, bool ignoreInvulnerability, string cause, out bool Skip)
        {
            Skip = false;
        }
        public override void OnProjectLoaded(Project project)
        {
        }
        public override void OnProjectDisposed()
        {
        }
    }
}
