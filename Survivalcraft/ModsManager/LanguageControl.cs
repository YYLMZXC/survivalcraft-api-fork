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


        public enum LanguageType
        {
            zh_CN,
            en_US,
            ot_OT
        }
        public static void Initialize(LanguageType languageType)
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
            ModsManager.modSettings.languageType = languageType;
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
        public static JsonObject JsonArraytoObj(JsonArray jsonArray)
        {
            JsonObject obj = new JsonObject();
            for (int i = 0; i < jsonArray.Count; i++)
            {
                obj.Add(i.ToString(), jsonArray[i]);
            }
            return obj;
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
                    if (obj is JsonArray) jsonobj = JsonArraytoObj(obj as JsonArray);
                    else jsonobj = obj as JsonObject;
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
                    if (obj is JsonArray) jsonobj = JsonArraytoObj(obj as JsonArray);
                    else jsonobj = obj as JsonObject;
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
