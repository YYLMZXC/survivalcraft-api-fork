using Jint;

namespace Game;

public class InterfaceImplementForJs(IModLoader parent) : SurvivalCraftModInterface(parent)
{
    public override void OnMinerDig(ComponentMiner miner, TerrainRaycastResult raycastResult, ref float digProgress,
        out bool isDigSucceed)
    {
        float DigProgress1 = digProgress;
        bool Digged1 = false;
        JsInterface.handlersDictionary["OnMinerDig"].ForEach(function =>
        {
            Digged1 |= JsInterface.Invoke(function, miner, raycastResult, DigProgress1).AsBoolean();
        });
        isDigSucceed = Digged1;
    }
}