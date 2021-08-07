using Engine;
using System;
using System.IO;
using System.Collections.Generic;
using Engine.Serialization;
using Engine.Media;
using Engine.Graphics;
using System.Xml.Linq;

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
        public bool Get(Type type, string name,out object obj)
        {
            obj = null;
            if (Entity.GetFile(name, out Stream stream))
            {
                obj = ContentManager.StreamConvertType(type, stream);
                return true;
            }
            return false;
        }
        public bool Get(string name, out Stream stream)
        {
            stream = null;
            if (Entity.GetFile(name, out stream))
            {
                return true;
            }
            return false;
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
            string fixname = string.Empty;
            switch (type.FullName)
            {
                case "Engine.Media.BitmapFont":
                    {
                        if (Resources.TryGetValue(name + ".png", out ContentInfo contentInfo1))
                        {
                            if (contentInfo1.Get(name + ".png", out Stream stream))
                            {
                                using (stream)
                                {
                                    if (contentInfo1.Get(name + ".lst", out Stream stream2))
                                    {
                                        using (stream)
                                        {
                                            return BitmapFont.Initialize(stream, stream2);
                                        }
                                    }
                                }
                            }
                        }
                        throw new Exception("Not found Resources:" + name + " for " + type.Name);
                    }
                case "Engine.Graphics.Shader": {
                        if (Resources.TryGetValue(name + ".psh", out ContentInfo contentInfo1))
                        {
                            if (contentInfo1.Get(name + ".psh", out Stream stream))
                            {
                                using (stream)
                                {
                                    if (contentInfo1.Get(name + ".vsh", out Stream stream2))
                                    {
                                        using (stream)
                                        {
                                            return new Shader(new StreamReader(stream).ReadToEnd(), new StreamReader(stream2).ReadToEnd(), null);
                                        }
                                    }
                                }
                            }
                        }
                        throw new Exception("Not found Resources:" + name + " for " + type.Name);
                    }
                case "Engine.Graphics.Texture2D": fixname=name+".png";break;
                case "System.String": fixname = name + ".txt"; break;
                case "Engine.Media.Image": fixname = name + ".png"; break;
                case "System.Xml.Linq.XElement": fixname = name + ".xml"; break;
                case "Engine.Graphics.Model": fixname = name + ".dae"; break;
                case "Game.Subtexture": if (name.StartsWith("Textures/Atlas/")) return TextureAtlasManager.GetSubtexture(name); else return new Subtexture(Get<Texture2D>(name),Vector2.Zero,Vector2.One);
                default: { break; }
            }
            if (Resources.TryGetValue(fixname, out ContentInfo contentInfo2))
            {
                if (contentInfo2.Get(fixname, out Stream stream))
                {
                    using (stream)
                    {//单文件转换
                        obj = StreamConvertType(type, stream);
                    }
                }
            }
            if (obj == null) throw new Exception("Not found Resources:" + name + " for " + type.Name);
            return obj;
        }
        public static object StreamConvertType(Type type,Stream stream)
        {
            switch (type.FullName)
            {
                case "Engine.Graphics.Texture2D": return Texture2D.Load(stream);
                case "System.String":return new StreamReader(stream).ReadToEnd();
                case "Engine.Media.Image": return Image.Load(stream);
                case "System.Xml.Linq.XElement": return XElement.Load(stream);
                case "Engine.Graphics.Model": return Model.Load(stream,true);
            }
            return null;
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
