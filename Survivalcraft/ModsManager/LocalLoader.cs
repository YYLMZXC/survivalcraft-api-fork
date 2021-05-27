using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Game
{

    public class LocalLoader
    {
        private AppDomain appDomain;
        private RemoteLoader remoteLoader;
        public Assembly Assembly => remoteLoader.assembly;
        public LocalLoader(string DomainName,byte[] data)
        {
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationName = DomainName;
            setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
            setup.PrivateBinPath = AppDomain.CurrentDomain.BaseDirectory;
            setup.CachePath = setup.ApplicationBase;
            setup.ShadowCopyFiles = "true";
            setup.ShadowCopyDirectories = setup.ApplicationBase;
            appDomain = AppDomain.CreateDomain(DomainName, null, setup);
            string name = Assembly.GetExecutingAssembly().GetName().FullName;
            remoteLoader = (RemoteLoader)appDomain.CreateInstanceAndUnwrap(name, typeof(RemoteLoader).FullName);
            LoadAssembly(data);
        }

        public void LoadAssembly(string fullName)
        {
            remoteLoader.LoadAssembly(fullName);
        }
        public void LoadAssembly(byte[] data)
        {
            remoteLoader.LoadAssembly(data);
        }
        public void Unload()
        {
            AppDomain.Unload(appDomain);
            appDomain = null;
        }

        public string FullName
        {
            get
            {
                return remoteLoader.FullName;
            }
        }
    }
}
