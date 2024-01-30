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
        public ModEntity ModEntity { get; set; }

        public void _OnLoaderInitialize()
        {
            ModInterfacesManager.RegisterInterface<InterfaceImplementForSurvivalCraft>(this);
        }
    }
}