using System.Collections.Generic;

namespace Game;

public interface IModLoader
{
    public ModEntity ModEntity { get; internal set; }
    void _OnLoaderInitialize();
}