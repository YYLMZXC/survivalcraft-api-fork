using System.Reflection;

namespace Game.IContentReader
{
    public class AssemblyReader : IContentReader
    {
        public string Type => "System.Reflection.Assembly";
        public string[] DefaultSuffix => ["dll","Exp"];
        public object Get(ContentInfo[] contents)
        {
            return Assembly.Load(ModsManager.StreamToBytes(contents[0].Duplicate()));
        }
    }
}