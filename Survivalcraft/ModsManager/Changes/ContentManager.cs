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
        public ContentInfo(string PackageName, string AbsolutePath_)
        {
            AbsolutePath = PackageName + ":"+ AbsolutePath_;
            Filename = Path.GetFileName(AbsolutePath);
            int pos = AbsolutePath_.LastIndexOf('.');
            ContentPath = pos > -1 ? AbsolutePath_.Substring(0, pos) : AbsolutePath_;
        }
        public void SetContentStream(Stream stream) {
            if (stream is MemoryStream) {
                ContentStream = stream as MemoryStream;
                ContentStream.Position = 0L;
            }
        }
        public Stream Duplicate() {
            if (ContentStream == null || !ContentStream.CanRead || !ContentStream.CanWrite) throw new Exception("ContentStream has been disposed");
            MemoryStream memoryStream = new MemoryStream();
            ContentStream.CopyTo(memoryStream);
            ContentStream.Position = 0L;
            memoryStream.Position = 0L;
            return memoryStream;
        }
        public void Dispose() {
            ContentStream?.Dispose();
        }
    }
    public static class ContentManager
    {
        internal static Dictionary<string, ContentInfo> ResourcesAll = new Dictionary<string, ContentInfo>();
        internal static Dictionary<string, ContentInfo> Resources = new Dictionary<string, ContentInfo>();
        internal static Dictionary<string, IContentReader.IContentReader> ReaderList = new Dictionary<string, IContentReader.IContentReader>();
        internal static Dictionary<string, object> Caches = new Dictionary<string, object>();
        public static void Initialize()
        {
            ReaderList.Clear();
            Resources.Clear();
            ResourcesAll.Clear();
            Caches.Clear();
        }
        public static T Get<T>(string name, string suffix = null, bool useCache = true) where T : class
        {
            return Get(typeof(T),name,suffix,useCache) as T;
        }
        public static object Get(Type type, string name, string suffix = null, bool useCache = false)
        {
            object obj = null;
            string packagename = string.Empty;
            bool flag = name.Contains(":");
            if (ReaderList.TryGetValue(type.FullName, out IContentReader.IContentReader reader))
            {
                reader.UseCache = useCache;
                List<ContentInfo> contents = new List<ContentInfo>();
                if (suffix == null)
                {
                    for (int i = 0; i < reader.DefaultSuffix.Length; i++)
                    {
                        string p = name + "." + reader.DefaultSuffix[i];
                        if (flag)
                        {
                            if (ResourcesAll.TryGetValue(p, out ContentInfo contentInfo))
                            {
                                contents.Add(contentInfo);
                            }

                        }
                        else
                        {
                            if (Resources.TryGetValue(p, out ContentInfo contentInfo))
                            {
                                contents.Add(contentInfo);
                            }
                        }
                    }
                }
                else {
                    string p = name + suffix;
                    if (flag)
                    {
                        if (ResourcesAll.TryGetValue(p, out ContentInfo contentInfo))
                        {
                            contents.Add(contentInfo);
                        }

                    }
                    else
                    {
                        if (Resources.TryGetValue(p, out ContentInfo contentInfo))
                        {
                            contents.Add(contentInfo);
                        }
                    }
                }
                if (contents.Count == 0) {//修正subtexture
                    contents.Add(new ContentInfo("survivalcraft", name));
                }
                obj = reader.Get(contents.ToArray());
                if (Caches.TryGetValue(name, out object o))
                {
                    Caches[name] = obj;
                }
                else Caches.Add(name, obj);
            }
            if (obj == null) throw new Exception("not found any res:" + name);
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
        public static void Add(ContentInfo contentInfo)
        {
            if (!ResourcesAll.TryGetValue(contentInfo.AbsolutePath, out ContentInfo info)) {
                ResourcesAll.Add(contentInfo.AbsolutePath,contentInfo);
            }
            string[] tmp = contentInfo.AbsolutePath.Split(new char[] { ':'});
            if (tmp.Length == 2) {
                if (!Resources.TryGetValue(tmp[1], out ContentInfo info2))
                {
                    Resources[tmp[1]] = contentInfo;
                }
                else Resources.Add(tmp[1], contentInfo);
            }
        }
        /// <summary>
        /// 可能需要带上文件后缀，即获取名字+获取的后缀
        /// </summary>
        /// <param name="name"></param>
        public static void Dispose(string name)
        {
            if (Caches.TryGetValue(name,out object obj)) {
                if (obj is IDisposable dis) {
                    dis.Dispose();
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
