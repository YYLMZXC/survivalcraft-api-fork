using System;
using System.Reflection;

namespace Game
{
    public class RemoteLoader : MarshalByRefObject
    {
        public Assembly assembly;

        public void LoadAssembly(string fullName)
        {
            assembly = Assembly.LoadFrom(fullName);
        }
        public void LoadAssembly(byte[] data)
        {
            assembly = Assembly.Load(data);
        }

        public string FullName
        {
            get { return assembly.FullName; }
        }
    }
}
