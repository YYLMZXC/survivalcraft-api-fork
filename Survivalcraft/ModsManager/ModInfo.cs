// Game.ModInfo
using System.Collections.Generic;
using System;
namespace Game{
    public class ModInfo
    {
        public string Name;
        public string Version;
        public string ApiVersion;
        public string Description;
        public string ScVersion;
        public string Link;
        public List<string> Dependencies = new List<string>();
    }
}
