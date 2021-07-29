using Engine;
using SimpleJson;
using System.Collections.Generic;
using System.IO;
using System;
namespace Game
{
    public static class LanguageControl
    {
        public static JsonObject KeyWords = new JsonObject();

        public enum LanguageType
        {
            zh_CN,
            en_US,
            ot_OT
        }
        public static void Initialize(LanguageType languageType)
        {
            KeyWords.Clear();
        }
        public static void loadJson(Stream stream)
        {
            string txt = new StreamReader(stream).ReadToEnd();
            if (txt.Length > 0)
            {//加载原版语言包
                var obj = SimpleJson.SimpleJson.DeserializeObject(txt);
                loadJsonLogic(KeyWords,obj);
            }
        }

        public static void loadJsonLogic(JsonObject node, object obj) {
            if (obj is JsonObject)
            {
                JsonObject jsonobj = obj as JsonObject;
                foreach (var item in jsonobj)
                {
                    if (item.Value is string)
                    {
                        if (node.ContainsKey(item.Key))
                        {
                            node[item.Key] = item.Value;
                        }
                        else node.Add(item.Key, item.Value);
                    }
                    else
                    {
                        JsonObject keys = new JsonObject();
                        if (node.ContainsKey(item.Key))
                        {
                            node[item.Key] = keys;
                        }
                        else node.Add(item.Key, keys);
                        loadJsonLogic(keys, item.Value);
                    }
                }
            }
            else if (obj is JsonArray)
            {
                JsonArray jsonArray = obj as JsonArray;
                for (int i = 0; i < jsonArray.Count; i++)
                {
                    if (jsonArray[i] is string)
                    {
                        if (node.ContainsKey(i.ToString()))
                        {
                            node[i.ToString()] = jsonArray[i];
                        }
                        else KeyWords.Add(i.ToString(), jsonArray[i]);
                    }
                    else {
                        JsonObject keys = new JsonObject();
                        if (node.ContainsKey(i.ToString()))
                        {
                            node[i.ToString()] = keys;
                        }else node.Add(i.ToString(), jsonArray[i]);
                        loadJsonLogic(keys, jsonArray[i]);
                    }
                }
            }
        }
        public static bool Get(out string result,params string[] keys) {
            int i = 0;
            JsonObject jsonobj = KeyWords;
            object obj = null;
            result = string.Empty;
            while (i < keys.Length)
            {
                if (jsonobj != null && jsonobj.ContainsKey(keys[i]))
                {
                    obj = jsonobj[keys[i]];
                    jsonobj = obj as JsonObject;
                }
                i++;
            }
            if (obj is string)
            {
                result = obj as string;
                return true;
            }
            result = keys[--i];
            return false;
        }
        public static JsonObject Get(params string[] keys)
        {
            int i = 0;
            object obj = null;
            JsonObject jsonobj = KeyWords;
            while (i < keys.Length)
            {
                if (jsonobj != null && jsonobj.ContainsKey(keys[i]))
                {
                    obj = jsonobj[keys[i]];
                    jsonobj = obj as JsonObject;
                }
                i++;
            }
            if (obj is JsonObject)
            {
                return obj as JsonObject;
            }
            return new JsonObject();
        }



        public static string LName()
        {
            return ModsManager.modSettings.languageType.ToString();
        }
        public static string Get(string className, int key)
        {//获得键值
            return Get(className, key.ToString());
        }
        public static string Get(string className, string key)
        {//获得键值
            if (Get(out string res, className, key.ToString()))
            {
                return res;
            }
            return key;
        }
        public static string GetBlock(string name, string prop)
        {
            string[] nm = name.Split(new char[] { ':' }, StringSplitOptions.None);

            if (Get(out string res, "Blocks", name, prop))
            {
                return res;
            }
            else if (nm.Length == 2 && Get(out string res2, "Blocks", string.Format("{0}:0", nm[0]), prop))
            {
                return res2;
            }
            return string.Empty;
        }
        public static string GetContentWidgets(string name, string prop)
        {
            if (Get(out string res, "ContentWidgets", name, prop))
            {
                return res;
            }
            return string.Empty;
        }
        public static string GetContentWidgets(string name, int pos)
        {
            if (Get(out string res, "ContentWidgets", pos.ToString()))
            {
                return res;
            }
            return string.Empty;
        }

        public static string GetDatabase(string name, string prop)
        {
            if (Get(out string res, "Database", name, prop))
            {
                return res;
            }
            return prop;
        }
        public static string GetFireworks(string name, string prop)
        {
            if (Get(out string res, "FireworksBlock", name, prop))
            {
                return res;
            }
            return string.Empty;
        }

    }
}
