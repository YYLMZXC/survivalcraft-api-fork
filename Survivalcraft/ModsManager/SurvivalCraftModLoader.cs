using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Engine.Media;
using GameEntitySystem;
using Jint;

namespace Game
{
    public class SurvivalCraftModLoader: IModLoader
    {
        public SurvivalCraftModLoader()
        {
            
        }
        public ModEntity ModEntity { get; set; }

        public void _OnLoaderInitialize()
        {
            ModInterfacesManager.RegisterInterface<SurvivalCraftModInterfaceImplement>(this);
        }
    }
}