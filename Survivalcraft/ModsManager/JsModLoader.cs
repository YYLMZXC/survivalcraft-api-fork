namespace Game;

public class JsModLoader : IModLoader
{
    public ModEntity ModEntity { get; set; }
    public void _OnLoaderInitialize()
    {
        ModInterfacesManager.RegisterInterface<InterfaceImplementForJs>(this);
    }
}