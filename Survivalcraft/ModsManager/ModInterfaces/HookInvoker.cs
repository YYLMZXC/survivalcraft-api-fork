namespace Game;

public delegate void HookInvoker<TInterface>(TInterface modInterface, out bool isContinueRequired) where TInterface : ModInterface;