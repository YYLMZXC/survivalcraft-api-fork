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
        public static string Ok = default;
        public static string Cancel = default;
        public static string None = default;
        public static string Nothing = default;
        public static string Error = default;
        public static string On = default;
        public static string Off = default;
        public static string Disable = default;
        public static string Enable = default;
        public static string Warning = default;
        public static string Back = default;
        public static string Allowed = default;
        public static string NAllowed = default;
        public static string Unknown = default;
        public static string Yes = default;
        public static string No = default;
        public static string Unavailable = default;
        public static string Exists = default;
        public static string Success = default;
        public static string Delete = default;
        public static List<string> LanguageTypes = new List<string>();

        public static void Initialize(string languageType)
        {
            Ok = default;
            Cancel = default;
            None = default;
            Nothing = default;
            Error = default;
            On = default;
            Off = default;
            Disable = default;
            Enable = default;
            Warning = default;
            Back = default;
            Allowed = default;
            NAllowed = default;
            Unknown = default;
            Yes = default;
            No = default;
            Unavailable = default;
            Exists = default;
            Success = default;
            Delete = default;
            KeyWords.Clear();
            ModsManager.SetConfig("Language", languageType);
        }
        public static void loadJson(Stream stream)
        {
            string txt = new StreamReader(stream).ReadToEnd();
            if (txt.Length > 0)
            {//加载原版语言包
                var obj = SimpleJson.SimpleJson.DeserializeObject(txt);
                loadJsonLogic(KeyWords, obj);
            }
            if (Ok == default) Ok = Get("Usual", "ok");
            if (Cancel == default) Cancel = Get("Usual", "cancel");
            if (None == default) None = Get("Usual", "none");
            if (Nothing == default) Nothing = Get("Usual", "nothing");
            if (Error == default) Error = Get("Usual", "error");
            if (On == default) On = Get("Usual", "on");
            if (Off == default) Off = Get("Usual", "off");
            if (Disable == default) Disable = Get("Usual", "disable");
            if (Enable == default) Enable = Get("Usual", "enable");
            if (Warning == default) Warning = Get("Usual", "warning");
            if (Back == default) Back = Get("Usual", "back");
            if (Allowed == default) Allowed = Get("Usual", "allowed");
            if (NAllowed == default) NAllowed = Get("Usual", "not allowed");
            if (Unknown == default) Unknown = Get("Usual", "unknown");
            if (Yes == default) Yes = Get("Usual", "yes");
            if (No == default) No = Get("Usual", "no");
            if (Unavailable == default) Unavailable = Get("Usual", "Unavailable");
            if (Exists == default) Exists = Get("Usual", "exist");
            if (Success == default) Success = Get("Usual", "success");
            if (Delete == default) Success = Get("Usual", "delete");
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
                        if (node.ContainsKey(item.Key))
                        {
                            loadJsonLogic(node[item.Key] as JsonObject,item.Value);
                        }
                        else node.Add(item.Key, item.Value);
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
                        else node.Add(i.ToString(), jsonArray[i]);
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
        public static string LName()
        {
            return ModsManager.Configs["Language"];
        }
        public static string Get(string className, int key)
        {//获得键值
            return Get(className, key.ToString());
        }
        public static string GetWorldPalette(int index)
        {
            return Get("WorldPalette", "Colors", index.ToString());
        }
        public static string Get(params string[] key)
        {
            return Get(out bool r, key);
        }
        public static string Get(out bool r, params string[] key)
        {//获得键值
            r = false;
            JsonObject obj = KeyWords;
            JsonArray arr = null;
            for (int i = 0; i < key.Length; i++)
            {
                bool flag = false;
                if (arr != null)
                {
                    int.TryParse(key[i], out int p);
                    object obj2 = arr[p];
                    if (obj2 is JsonObject jo)
                    {
                        obj = jo;
                        arr = null;
                        flag = true;
                    }
                    else if (obj2 is JsonArray ja)
                    {
                        obj = null;
                        arr = ja;
                        flag = true;
                    }
                    else
                    {
                        r = true;
                        return obj2.ToString();
                    }
                }
                else
                {
                    if (obj.TryGetValue(key[i], out object obj2))
                    {
                        if (obj2 is JsonObject jo)
                        {
                            obj = jo;
                            arr = null;
                            flag = true;
                        }
                        else if (obj2 is JsonArray ja)
                        {
                            obj = null;
                            arr = ja;
                            flag = true;
                        }
                        else
                        {
                            r = true;
                            return obj2.ToString();
                        }
                    }
                }
                if (!flag)
                {
                    return key[i];
                }
            }
            string str = "";
            foreach (string s in key) str += s + ":";
            return str;
        }
        public static string GetBlock(string blockName, string prop)
        {
            if (TryGetBlock(blockName, prop, out var result))
            {
                return result;
            }
            return result;
        }
        public static bool TryGetBlock(string blockName, string prop, out string result)
        {
            string[] nm = blockName.Split(new char[] { ':' }, StringSplitOptions.None);
            result = Get(out bool r, "Blocks", nm.Length < 2 ? (blockName + ":0") : (nm[0] + ":0"), prop);
            return r;
        }
        public static string GetContentWidgets(string name, string prop)
        {
            return Get("ContentWidgets", name, prop);
        }
        public static string GetContentWidgets(string name, int pos)
        {
            return Get("ContentWidgets", name, pos.ToString());
        }
        public static string GetDatabase(string name, string prop)
        {
            return Get("Database", name, prop);
        }
        public static string GetFireworks(string name, string prop)
        {
            return Get("FireworksBlock", name, prop);
        }
    }
}
