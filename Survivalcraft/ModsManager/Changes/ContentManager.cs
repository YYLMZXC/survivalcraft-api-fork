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
    public class ContentInfo
    {
        public ModEntity Entity;
        public string Filename;
        public string ContentPath;
        public ContentInfo(ModEntity entity, string AbsolutePath_)
        {
            Entity = entity;
            Filename = Path.GetFileName(AbsolutePath_);
            int pos = AbsolutePath_.LastIndexOf('.');
            ContentPath = AbsolutePath_.Substring(0, pos);
        }
        public bool Get(Type type, string name, out object obj)
        {
            obj = null;
            if (Entity.GetAssetsFile(name, out Stream stream))
            {
                obj = ContentManager.StreamConvertType(type, stream);
                return true;
            }
            return false;
        }
        public bool Get(string name, out Stream stream)
        {
            stream = null;
            if (Entity.GetAssetsFile(name, out stream))
            {
                return true;
            }
            return false;
        }
    }
    public static class ContentManager
    {
        internal static Dictionary<string, ContentInfo> Resources = new Dictionary<string, ContentInfo>();
        internal static Dictionary<string, object> ResourcesCaches = new Dictionary<string, object>();
        public static void Initialize()
        {
            Resources.Clear();
        }
        /// <summary>
        /// 获取资源
        /// </summary>
        /// <typeparam name="T">要转换的类型</typeparam>
        /// <param name="name">资源名称</param>
        /// <param name="useCache">是否使用缓存</param>
        /// <returns></returns>
        public static T Get<T>(string name, string prefix = null, bool useCache = true) where T : class
        {
            try
            {
                return Get(typeof(T), name, prefix, useCache) as T;
            }
            catch (Exception e)
            {
                LoadingScreen.Warning("Loading Resouce " + name + " error." + e.Message);
                return null;
            }
        }

        public static object Get(Type type, string name, string prefix = null, bool useCache = false)
        {
            string fixname = string.Empty;
            object obj = null;
            object obj1 = null;
            string cacheName = name + prefix;
            if (useCache && ResourcesCaches.TryGetValue(cacheName, out obj1))
            {
                return obj1;
            }
            if (name.Contains(":"))
            { //带命名空间的解析
                string[] spl = name.Split(new char[] { ':' }, StringSplitOptions.None);
                string ModSpace = spl[0];
                name = spl[1];
                if (ModsManager.GetModEntity(ModSpace, out ModEntity modEntity))
                {
                    switch (type.FullName)
                    {
                        case "Engine.Media.BitmapFont":
                            {
                                if (modEntity.GetAssetsFile(name + ".png", out Stream stream))
                                {
                                    using (stream)
                                    {
                                        if (modEntity.GetAssetsFile(name + ".lst", out Stream stream2))
                                        {
                                            BitmapFont bitmapFont = BitmapFont.Initialize(stream, stream2);
                                            stream2.Close();
                                            if (useCache)
                                            {
                                                if (obj1 == null) ResourcesCaches.Add(cacheName, bitmapFont);
                                                else ResourcesCaches[cacheName] = bitmapFont;
                                            }
                                            return bitmapFont;
                                        }
                                    }
                                }
                                throw new Exception("Not found Resources:" + name + " for " + type.FullName);
                            }
                        case "Engine.Graphics.Shader":
                            {
                                if (modEntity.GetAssetsFile(name + ".psh", out Stream stream))
                                {
                                    using (stream)
                                    {
                                        if (modEntity.GetAssetsFile(name + ".vsh", out Stream stream2))
                                        {
                                            Shader shader = new Shader(new StreamReader(stream).ReadToEnd(), new StreamReader(stream2).ReadToEnd(), new ShaderMacro[] { new ShaderMacro(name) });
                                            stream2.Close();
                                            if (useCache)
                                            {
                                                if (obj1 == null) ResourcesCaches.Add(cacheName, shader);
                                                else ResourcesCaches[cacheName] = shader;

                                            }
                                            return shader;
                                        }
                                    }
                                }
                                throw new Exception("Not found Resources:" + name + " for " + type.FullName);
                            }
                        default:
                            {
                                switch (type.FullName)
                                {
                                    case "Engine.Media.StreamingSource":
                                    case "Engine.Audio.SoundBuffer": if (string.IsNullOrEmpty(prefix)) throw new Exception("You must specify a file type."); else fixname = name + prefix; break;
                                    case "Game.MtllibStruct": if (string.IsNullOrEmpty(prefix)) fixname = name + ".mtl"; else fixname = name + prefix; break;
                                    case "Engine.Graphics.Texture2D": if (string.IsNullOrEmpty(prefix)) fixname = name + ".png"; else fixname = name + prefix; break;
                                    case "System.String": if (string.IsNullOrEmpty(prefix)) fixname = name + ".txt"; else fixname = name + prefix; break;
                                    case "Engine.Media.Image": if (string.IsNullOrEmpty(prefix)) fixname = name + ".png"; else fixname = name + prefix; break;
                                    case "System.Xml.Linq.XElement": if (string.IsNullOrEmpty(prefix)) fixname = name + ".xml"; else fixname = name + prefix; break;
                                    case "Engine.Graphics.Model": if (string.IsNullOrEmpty(prefix)) fixname = name + ".dae"; else fixname = name + prefix; break;
                                    case "Game.ObjModel": if (string.IsNullOrEmpty(prefix)) fixname = name + ".obj"; else fixname = name + prefix; break;
                                    case "SimpleJson.JsonObject":
                                    case "Game.JsonModel": if (string.IsNullOrEmpty(prefix)) fixname = name + ".json"; else fixname = name + prefix; break;
                                    case "Game.Subtexture": if (name.StartsWith("Textures/Atlas/")) obj = TextureAtlasManager.GetSubtexture(name); else obj = new Subtexture(Get<Texture2D>(name, null, useCache), Vector2.Zero, Vector2.One); break;
                                }
                                if (modEntity.GetAssetsFile(fixname, out Stream stream))
                                {
                                    obj = StreamConvertType(type, stream);
                                }
                                else throw new Exception("Not found Resources:" + ModSpace + ":" + name + " for " + type.FullName);
                                if (useCache)
                                {

                                    if (obj1 == null) ResourcesCaches.Add(cacheName, obj);
                                    else ResourcesCaches[cacheName] = obj;
                                }
                                return obj;
                            }
                    }
                }
                else
                {
                    throw new Exception("not found modspace:" + ModSpace);
                }
            }
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
                                        BitmapFont bitmapFont = BitmapFont.Initialize(stream, stream2);
                                        stream2.Close();
                                        if (useCache)
                                        {
                                            if (obj1 == null) ResourcesCaches.Add(cacheName, bitmapFont);
                                            else ResourcesCaches[cacheName] = bitmapFont;
                                        }
                                        return bitmapFont;
                                    }
                                }
                            }
                        }
                        throw new Exception("Not found Resources:" + name + " for " + type.FullName);
                    }
                case "Engine.Graphics.Shader":
                    {
                        if (Resources.TryGetValue(name + ".psh", out ContentInfo contentInfo1))
                        {
                            if (contentInfo1.Get(name + ".psh", out Stream stream))
                            {
                                using (stream)
                                {
                                    if (contentInfo1.Get(name + ".vsh", out Stream stream2))
                                    {
                                        Shader shader = new Shader(new StreamReader(stream).ReadToEnd(), new StreamReader(stream2).ReadToEnd(), new ShaderMacro[] { new ShaderMacro(name) });
                                        stream2.Close();
                                        if (useCache)
                                        {

                                            if (obj1 == null) ResourcesCaches.Add(cacheName, shader);
                                            else ResourcesCaches[cacheName] = shader;
                                        }
                                        return shader;
                                    }
                                }
                            }
                        }
                        throw new Exception("Not found Resources:" + name + " for " + type.FullName);
                    }
                case "Engine.Media.StreamingSource":
                case "Engine.Audio.SoundBuffer": if (string.IsNullOrEmpty(prefix)) throw new Exception("You must specify a file type."); else fixname = name + prefix; break;
                case "Game.MtllibStruct": if (string.IsNullOrEmpty(prefix)) fixname = name + ".mtl"; else fixname = name + prefix; break;
                case "Engine.Graphics.Texture2D": if (string.IsNullOrEmpty(prefix)) fixname = name + ".png"; else fixname = name + prefix; break;
                case "System.String": if (string.IsNullOrEmpty(prefix)) fixname = name + ".txt"; else fixname = name + prefix; break;
                case "Engine.Media.Image": if (string.IsNullOrEmpty(prefix)) fixname = name + ".png"; else fixname = name + prefix; break;
                case "System.Xml.Linq.XElement": if (string.IsNullOrEmpty(prefix)) fixname = name + ".xml"; else fixname = name + prefix; break;
                case "Engine.Graphics.Model": if (string.IsNullOrEmpty(prefix)) fixname = name + ".dae"; else fixname = name + prefix; break;
                case "Game.ObjModel": if (string.IsNullOrEmpty(prefix)) fixname = name + ".obj"; else fixname = name + prefix; break;
                case "SimpleJson.JsonObject":
                case "Game.JsonModel": if (string.IsNullOrEmpty(prefix)) fixname = name + ".json"; else fixname = name + prefix; break;
                case "Game.Subtexture": if (name.StartsWith("Textures/Atlas/")) obj = TextureAtlasManager.GetSubtexture(name); else obj = new Subtexture(Get<Texture2D>(name, null, useCache), Vector2.Zero, Vector2.One); break;
                default: { break; }
            }
            if (Resources.TryGetValue(fixname, out ContentInfo contentInfo3))
            {
                if (contentInfo3.Get(fixname, out Stream stream))
                {
                    using (stream)
                    {//单文件转换
                        obj = StreamConvertType(type, stream);
                    }
                }
            }
            if (useCache)
            {
                if (obj1 == null) ResourcesCaches.Add(cacheName, obj);
                else ResourcesCaches[cacheName] = obj;

            }
            if (obj == null) throw new Exception("Not found Resources:" + name + " for " + type.FullName);
            return obj;
        }
        public static object StreamConvertType(Type type, Stream stream)
        {
            switch (type.FullName)
            {
                case "SimpleJson.JsonObject": return SimpleJson.SimpleJson.DeserializeObject(new StreamReader(stream).ReadToEnd());
                case "Engine.Media.StreamingSource": return SoundData.Stream(stream);
                case "Engine.Audio.SoundBuffer": return Engine.Audio.SoundBuffer.Load(stream);
                case "Engine.Graphics.Texture2D": return Texture2D.Load(stream);
                case "System.String": return new StreamReader(stream).ReadToEnd();
                case "Engine.Media.Image": return Image.Load(stream);
                case "Game.ObjModel": return ObjModelReader.Load(stream);
                case "Game.JsonModel": return JsonModelReader.Load(stream);
                case "System.Xml.Linq.XElement": return XElement.Load(stream);
                case "Engine.Graphics.Model": return Model.Load(stream, true);
                case "Game.MtllibStruct": return MtllibStruct.Load(stream);
            }
            return null;
        }
        public static void Add(ModEntity entity, string name)
        {
            if (Resources.TryGetValue(name, out ContentInfo contentInfo))
            {
                Resources[name] = new ContentInfo(entity, name);
            }
            else
                Resources.Add(name, new ContentInfo(entity, name));
        }
        /// <summary>
        /// 可能需要带上文件后缀，即获取名字+获取的后缀
        /// </summary>
        /// <param name="name"></param>
        public static void Dispose(string name)
        {
            foreach (var obj in ResourcesCaches)
            {
                if (name == obj.Key)
                {
                    IDisposable disposable = obj.Value as IDisposable;
                    if (disposable != null) disposable.Dispose();
                    break;
                }
            }
        }

        public static bool IsContent(object content)
        {
            foreach (var obj in ResourcesCaches.Values)
            {
                if (obj == content) return true;
            }
            return false;
        }

        public static ReadOnlyList<ContentInfo> List()
        {
            return new ReadOnlyList<ContentInfo>(Resources.Values.ToDynamicArray());
        }

        public static ReadOnlyList<ContentInfo> List(string directory)
        {
            List<ContentInfo> contents = new List<ContentInfo>();
            foreach (var content in Resources.Values)
            {
                if (content.ContentPath.StartsWith(directory)) contents.Add(content);
            }
            return new ReadOnlyList<ContentInfo>(contents);
        }
    }
}
