using System;
using Engine;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
    public class SubsystemBlocksManager : Subsystem
    {
        //以ClassName, BlockContent的形式存储和读取方块信息

        /*流程：
         * 在加载世界时Load调用（需要将这个调整为高优先级加载）
         * BlocksManager加载原版和所有mod的静态ID方块
         * 调用SubsystemBlocksManager.CallAllocate()调用分配动态ID方块
         * BlocksManager加载项目声明的动态ID方块
         * BlocksManager加载剩下的动态ID方块
         * PostProcess进行后处理
         */

        public Dictionary<string, int> DynamicBlockNameToIndex = new Dictionary<string, int>();
        public override void Load(ValuesDictionary valuesDictionary)
        {
            BlocksManager.InitializeBlocks(this);
            BlocksManager.PostProcessBlocksLoad();
        }

        public void CallAllocate()
        {

        }

        public override void Save(ValuesDictionary valuesDictionary)
        {
            base.Save(valuesDictionary);
        }
    }
}
