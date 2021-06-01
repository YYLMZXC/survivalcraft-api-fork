using Engine;

namespace Game
{
    public class FastDebugModEntity :ModEntity
    {

        public override void LoadDll()
        {
            Storage.ListFileNames(ModsManager.ModsPath);   
            LoadDllLogic();
        }


    }
}
