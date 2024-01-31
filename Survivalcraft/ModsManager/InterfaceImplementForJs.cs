using Engine;
using GameEntitySystem;
using Jint;

namespace Game;

public class InterfaceImplementForJs(IModLoader parent) : SurvivalCraftModInterface(parent)
{
    public override void OnMinerDig(ComponentMiner miner, TerrainRaycastResult raycastResult, ref float digProgress,
        out bool isDigSucceed)
    {
        float digProgress1 = digProgress;
        var digSucceed = false;
        JsInterface.handlersDictionary["OnMinerDig"].ForEach(function =>
        {
            digSucceed |= function.Invoke(miner, raycastResult, digProgress1)?.AsBoolean() ?? false;
        });
        isDigSucceed = digSucceed;
    }
    public override void OnMinerPlace(ComponentMiner miner, TerrainRaycastResult raycastResult, int x, int y, int z,
        int value, out bool placed)
    {
        var placed1 = false;
        JsInterface.handlersDictionary["OnMinerPlace"].ForEach(function =>
        {
            placed1 |= function.Invoke(miner, raycastResult, x, y, z, value)?.AsBoolean() ?? false;
        });
        placed = placed1;
    }
    public override bool OnPlayerSpawned(PlayerData.SpawnMode spawnMode, ComponentPlayer componentPlayer,
        Vector3 position)
    {
        JsInterface.handlersDictionary["OnPlayerSpawned"].ForEach(function =>
        {
            function.Invoke(spawnMode, componentPlayer, position);
        });
        return false;
    }

    public override void OnPlayerDead(PlayerData playerData)
    {
        JsInterface.handlersDictionary["OnPlayerDead"].ForEach(function =>
        {
            function.Invoke(playerData);
        });
    }
    public override bool AttackBody(ComponentBody target, ComponentCreature attacker, Vector3 hitPoint,
        Vector3 hitDirection, ref float attackPower, bool isMeleeAttack)
    {
        float attackPower1 = attackPower;
        var flag = false;
        JsInterface.handlersDictionary["OnMinerPlace"].ForEach(function =>
        {
            flag |= function.Invoke(target, attacker, hitPoint, hitDirection, attackPower1,
                isMeleeAttack)?.AsBoolean() ?? false;
        });
        return flag;
    }

    public override void OnCreatureInjure(ComponentHealth componentHealth, float amount, ComponentCreature attacker,
        bool ignoreInvulnerability, string cause, out bool skipVanilla)
    {
        var skip1 = false;
        JsInterface.handlersDictionary["OnCreatureInjure"].ForEach(func =>
        {
            skip1 |= func.Invoke(componentHealth, amount, attacker, ignoreInvulnerability, cause)?.AsBoolean() ?? false;
        });
        skipVanilla = skip1;
    }

    public override void OnProjectLoaded(Project project)
    {
        JsInterface.handlersDictionary["OnProjectLoaded"].ForEach(function =>
        {
            function.Invoke(project);

        });
    }

    public override void OnProjectDisposed()
    {
        JsInterface.handlersDictionary["OnProjectLoaded"].ForEach(function => { JsInterface.Invoke(function); });
    }
    
    public void Register(string hookName)
    {
        RegisterHook(hookName);
    }
}