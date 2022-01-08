using Engine;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Game
{
    public class ShaderCodeManager
    {
        public static string GetFast(string fname)
        {
            string shaderText = string.Empty;
            string[] parameters = fname.Split('.');
            if(parameters.Length > 1)
                shaderText = ModsManager.GetInPakOrStorageFile<string>(parameters[0], "." + parameters[1]);
            return shaderText;
        }

        public static string Get(string fname)
        {
            string shaderText = string.Empty;
            shaderText = GetIncludeText(shaderText, fname, false);
            return shaderText;
        }

        public static string GetExternal(string fname)
        {
            string shaderText = string.Empty;
            shaderText = GetIncludeText(shaderText, fname, true);
            return shaderText;
        }

        public static string GetIncludeText(string shaderText, string includefname, bool external)
        {
            string includeText = string.Empty;
            string shaderTextTemp = string.Empty;
            try
            {
                if (external)
                {
                    string path =ModsManager.IsAndroid ? "android:/SurvivalCraft2.2/" : "app:/";
                    Stream stream = Storage.OpenFile(Storage.CombinePaths(path, includefname), OpenFileMode.Read);
                    StreamReader streamReader = new StreamReader(stream);
                    shaderTextTemp = streamReader.ReadToEnd();
                }
                else
                {
                    if (includefname.Contains(".txt"))
                    {
                        includefname = includefname.Split(new char[1] { '.' })[0];
                        shaderTextTemp = ContentManager.Get<string>(includefname);
                    }
                    else
                    {
                        shaderTextTemp = GetFast(includefname);
                    }
                }
                if (shaderTextTemp == string.Empty) return string.Empty;
                shaderTextTemp = shaderTextTemp.Replace("\n", "$");
                string[] lines = shaderTextTemp.Split(new char[1] { '$' });
                for (int l = 0; l < lines.Length; l++)
                {
                    if (lines[l].Contains("#include"))
                    {
                        Regex regex = new Regex("\"[^\"]*\"");
                        string fname = regex.Match(lines[l]).Value.Replace("\"", "");
                        includeText += GetIncludeText(shaderText, fname, external);
                    }
                    else
                    {
                        if (!ModsManager.IsAndroid)
                        {
                            includeText += lines[l].Replace("highp", "").Replace("lowp","").Replace("mediump","") + "\n";
                        }
                        else
                        {
                            includeText += lines[l] + "\n";
                        }
                    }
                }
                shaderText += includeText;
            }
            catch
            {
            }
            return shaderText;
        }
    }
}