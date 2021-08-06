using Engine;
using System;
using System.IO;
using System.Collections.Generic;
using Engine.Serialization;
using Engine.Media;

namespace Game
{
    public class ContentInfo { 
        public ModEntity Entity;
        public string Filename;
        public string AbsolutePath;
        public ContentInfo(ModEntity entity, string filename)
        {
            Entity = entity;
            Filename = filename;
        }
        public object Get(Type type, string name)
        {
            object obj = null;
            if (Entity.GetFile(name, out Stream stream))
            {
                obj = ContentSerializer.StreamConvertType(type, name, stream);
            }
            return obj;
        }
    }
    public static class ContentManager
    {
        internal static Dictionary<string, ContentInfo> Resources = new Dictionary<string, ContentInfo>();

        public static void Initialize()
        {



        }

        public static T Get<T>(string name) where T : class
        {
            return Get(typeof(T),name) as T;
        }

        public static object Get(Type type, string name)
        {
            object obj = null;
            if (Resources.TryGetValue(name, out ContentInfo contentInfo)) {
                if (contentInfo.Entity.GetFile(name,out Stream stream)) {
                    using (stream) {
                        obj = ContentSerializer.StreamConvertType(type,name,stream);
                    }
                }            
            }
            return obj;
        }

        

        public static void Add(ModEntity entity,string name)
        {
            if (Resources.TryGetValue(name, out ContentInfo contentInfo))
            {
                Resources[name] = new ContentInfo(entity, name);
            }
            else
                Resources.Add(name, new ContentInfo(entity, name));
        }

        public static void Dispose(string name)
        {

        }

        public static bool IsContent(object content)
        {
            return false;
        }

        public static ReadOnlyList<ContentInfo> List()
        {
            return new ReadOnlyList<ContentInfo>(Resources.Values.ToDynamicArray());
        }

        public static ReadOnlyList<ContentInfo> List(string directory)
        {
            List<ContentInfo> contents = new List<ContentInfo>();
            foreach (var content in Resources.Values) {
                if(content.AbsolutePath.StartsWith(directory))contents.Add(content);            
            }
            return new ReadOnlyList<ContentInfo>(contents);
        }
    }
}
