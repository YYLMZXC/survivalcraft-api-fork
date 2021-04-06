using Engine;
using Engine.Content;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Game
{
    public static class ContentManager
    {
        public static bool Loaded = false;
        public static void Initialize()
        {
            if(Loaded)return;
            ContentCache.AddPackage("app:/Content.pak", Encoding.UTF8.GetBytes(Pad()), new byte[1]
            {
                63
            });
            ModsManager.Initialize();
            LanguageControl.init(ModsManager.modSettings.languageType);
            List<FileEntry> list = ModsManager.GetEntries(".pak");
            foreach (FileEntry fileEntry in list)
            {
                ContentCache.AddPackage(() => fileEntry.Stream, Encoding.UTF8.GetBytes(Pad()), new byte[1]
            {
                63
            });
            }
            Loaded = true;
        }

        public static object Get(string name)
        {
            return ContentCache.Get(name);
        }

        public static object Get(Type type, string name)
        {
            if (type == typeof(Subtexture))
            {
                return TextureAtlasManager.GetSubtexture(name);
            }
            if (type == typeof(string) && name.StartsWith("Strings/"))
            {
                return StringsManager.GetString(name.Substring(8));
            }
            object obj = Get(name);
            if (!type.GetTypeInfo().IsAssignableFrom(obj.GetType().GetTypeInfo()))
            {
                throw new InvalidOperationException(string.Format(LanguageControl.Get("ContentManager", "1"), name, obj.GetType().FullName, type.FullName));
            }
            return obj;
        }

        public static T Get<T>(string name)
        {
            return (T)Get(typeof(T), name);
        }

        public static void Dispose(string name)
        {
            ContentCache.Dispose(name);
        }

        public static bool IsContent(object content)
        {
            return ContentCache.IsContent(content);
        }

        public static ReadOnlyList<ContentInfo> List()
        {
            return ContentCache.List();
        }

        public static ReadOnlyList<ContentInfo> List(string directory)
        {
            return ContentCache.List(directory);
        }

        public static string Pad()
        {
            string text = string.Empty;
            string text2 = "0123456789abdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            Random random = new Random(171);
            for (int i = 0; i < 229; i++)
            {
                text += text2[random.Int(text2.Length)].ToString();
            }
            return text;
        }
    }
}
