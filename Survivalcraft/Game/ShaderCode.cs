using Engine;
using System;
using System.IO;

namespace Game
{
    public class ShaderCode
    {
        public static string Get(string fname)
        {
            string shaderText = string.Empty;
            string[] parameters = fname.Split('.');
            if(parameters.Length > 1)
                shaderText = ModsManager.GetInPakOrStorageFile<string>(parameters[0], "." + parameters[1]);
            return shaderText;
        }

        public static string GetFast(string fname)
        {
            string shaderText = string.Empty;
            try
            {
                string path = (Environment.CurrentDirectory == "/") ? "android:/SurvivalCraft2.2/" : "app:/";
                Stream stream = Storage.OpenFile(Storage.CombinePaths(path, fname), OpenFileMode.Read);
                StreamReader streamReader = new StreamReader(stream);
                shaderText = streamReader.ReadToEnd();
            }
            catch
            {
            }
            return shaderText;
        }
    }
}