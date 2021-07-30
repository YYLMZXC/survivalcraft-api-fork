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

        public static string Ok = string.Empty;
        public static string Cancel = string.Empty;
        public static string None = string.Empty;
        public static string Nothing = string.Empty;
        public static string Error = string.Empty;
        public static string On = string.Empty;
        public static string Off = string.Empty;
        public static string Disable = string.Empty;
        public static string Enable = string.Empty;
        public static string Warning = string.Empty;
        public static string Back = string.Empty;
        public static string Allowed = string.Empty;
        public static string NAllowed = string.Empty;
        public static string Unknown = string.Empty;
        public static string Yes = string.Empty;
        public static string No = string.Empty;
        public static string Unavailable = string.Empty;
        public static string Exists = string.Empty;
        public static string Success = string.Empty;
        public static string Delete = string.Empty;


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
                loadJsonLogic(KeyWords, obj);
            }
            if (Ok == string.Empty) Ok = Get("Usual", "ok");
            if (Cancel == string.Empty) Cancel = Get("Usual", "cancel");
            if (None == string.Empty) None = Get("Usual", "none");
            if (Nothing == string.Empty) Nothing = Get("Usual", "nothing");
            if (Error == string.Empty) Error = Get("Usual", "error");
            if (On == string.Empty) On = Get("Usual", "on");
            if (Off == string.Empty) Off = Get("Usual", "off");
            if (Disable == string.Empty) Disable = Get("Usual", "disable");
            if (Enable == string.Empty) Enable = Get("Usual", "enable");
            if (Warning == string.Empty) Warning = Get("Usual", "warning");
            if (Back == string.Empty) Back = Get("Usual", "back");
            if (Allowed == string.Empty) Allowed = Get("Usual", "allowed");
            if (NAllowed == string.Empty) NAllowed = Get("Usual", "not allowed");
            if (Unknown == string.Empty) Unknown = Get("Usual", "unknown");
            if (Yes == string.Empty) Yes = Get("Usual", "yes");
            if (No == string.Empty) No = Get("Usual", "no");
            if (Unavailable == string.Empty) Unavailable = Get("Usual", "Unavailable");
            if (Exists == string.Empty) Exists = Get("Usual", "exist");
            if (Success == string.Empty) Success = Get("Usual", "success");
            if (Delete == string.Empty) Success = Get("Usual", "delete");
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
