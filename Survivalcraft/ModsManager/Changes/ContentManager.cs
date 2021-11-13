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
        public MemoryStream ContentStream;
        public string AbsolutePath;
        public string ContentPath;
        public string Filename;
        public object Instance;
        public ContentInfo(string AbsolutePath_)
        {
            AbsolutePath = AbsolutePath_;
            int pos = AbsolutePath_.LastIndexOf('.');
            ContentPath = pos > -1 ? AbsolutePath_.Substring(0, pos) : AbsolutePath_;
            Filename = Path.GetFileName(AbsolutePath);
        }
        public void SetContentStream(Stream stream)
        {
            if (stream is MemoryStream)
            {
                ContentStream = stream as MemoryStream;
                ContentStream.Position = 0L;
            }
            else
            {
                throw new Exception("Can't set ContentStream width type " + stream.GetType().Name);
            }
        }
        public Stream Duplicate()
        {
            if (ContentStream == null || !ContentStream.CanRead || !ContentStream.CanWrite) throw new Exception("ContentStream has been disposed");
            MemoryStream memoryStream = new MemoryStream();
            ContentStream.CopyTo(memoryStream);
            ContentStream.Position = 0L;
            memoryStream.Position = 0L;
            return memoryStream;
        }
        public void Dispose()
        {
            ContentStream?.Dispose();
        }
    }
    public static class ContentManager
    {
        internal static Dictionary<string, ContentInfo> Resources = new Dictionary<string, ContentInfo>();
        internal static Dictionary<string, IContentReader.IContentReader> ReaderList = new Dictionary<string, IContentReader.IContentReader>();
        internal static Dictionary<string, object> Caches = new Dictionary<string, object>();
        internal static object syncobj = new object();
        public static void Initialize()
        {
            ReaderList.Clear();
            Resources.Clear();
            Caches.Clear();
        }
        public static T Get<T>(string name, string suffix = null) where T : class
        {
            return Get(typeof(T), name, suffix) as T;
        }
        public static object Get(Type type, string name, string suffix = null)
        {
            lock (syncobj)
            {
                string fullname = suffix == null ? name : name + "." + suffix;
                if (Caches.TryGetValue(fullname, out var o)) return o;
                object obj = null;
                if (type == typeof(Subtexture))
                {
                    return TextureAtlasManager.GetSubtexture(name);
                }
                if (ReaderList.TryGetValue(type.FullName, out IContentReader.IContentReader reader))
                {
                    List<ContentInfo> contents = new List<ContentInfo>();
                    if (suffix == null)
                    {
                        for (int i = 0; i < reader.DefaultSuffix.Length; i++)
                        {
                            string p = name + "." + reader.DefaultSuffix[i];
                            if (Resources.TryGetValue(p, out ContentInfo contentInfo))
                            {
                                contents.Add(contentInfo);
                            }
                        }
                    }
                    else
                    {
                        string p = name + suffix;
                        if (Resources.TryGetValue(p, out ContentInfo contentInfo))
                        {
                            contents.Add(contentInfo);
                        }
                    }
                    if (contents.Count == 0)
                    {//没有找到对应资源?
                        contents.Add(new ContentInfo(name));
                    }
                    obj = reader.Get(contents.ToArray());
                }
                if (obj == null) throw new Exception("not found any res:" + name);
                Caches.Add(fullname, obj);
                return obj;
            }
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
        public static void Add(ContentInfo contentInfo)
        {
            lock (syncobj)
            {
                if (!Resources.TryGetValue(contentInfo.AbsolutePath, out ContentInfo info))
                {
                    Resources.Add(contentInfo.AbsolutePath, contentInfo);
                }
            }
        }
        /// <summary>
        /// 可能需要带上文件后缀，即获取名字+获取的后缀
        /// </summary>
        /// <param name="name"></param>
        public static void Dispose(string name)
        {
            lock (syncobj)
            {
                if (Caches.TryGetValue(name, out object obj))
                {
                    if (obj is IDisposable dis)
                    {
                        dis.Dispose();
                    }
                    Caches.Remove(name);
                }
            }
        }
        public static bool IsContent(object content)
        {
            foreach (var obj in Caches.Values)
            {
                if (obj == content) return true;
            }
            return false;
        }
        public static void Display_DeviceReset()
        {
            foreach (var item in Caches)
            {
                if (item.Value is Texture2D || item.Value is Model || item.Value is BitmapFont)
                {
                    Caches[item.Key] = Get(item.Value.GetType(), item.Key);
                }
            }
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
