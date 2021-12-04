namespace Game
{
    public class HardwareManager
    {
        public void Vibrate(long ms)
        {
#if android
            Engine.Window.Activity.Vibrate(ms);
#endif
        }
    }
}
