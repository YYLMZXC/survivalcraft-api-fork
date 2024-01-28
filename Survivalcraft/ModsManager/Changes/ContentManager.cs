using Engine;
using Engine.Graphics;
using Engine.Media;
using System;
using System.Collections.Generic;
using System.IO;

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
			MemoryStream memoryStream = new();
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
		internal static Dictionary<string, ContentInfo> Resources = [];
		internal static Dictionary<string, IContentReader.IContentReader> ReaderList = [];
		internal static Dictionary<string, List<object>> Caches = [];
		internal static object syncobj = new();
		public static void Initialize()
		{
			ReaderList.Clear();
			Resources.Clear();
			Caches.Clear();
			Display.DeviceReset += Display_DeviceReset;
		}
		public static T Get<T>(string name, string suffix = null) where T : class
		{
			return Get(typeof(T), name, suffix) as T;
		}
		public static object Get(Type type, string name, string suffix = null)
		{
			lock (syncobj)
			{
				object obj = null;
				string key = suffix == null ? name : name + "." + suffix;
				if (type == typeof(Subtexture))
				{
					return TextureAtlasManager.GetSubtexture(name);
				}
				if (Caches.TryGetValue(key, out var cacheList)) obj = cacheList.Find(f => f.GetType() == type);
				if (obj != null) return obj;
				if (ReaderList.TryGetValue(type.FullName, out IContentReader.IContentReader reader))
				{
					List<ContentInfo> contents = [];
					string possibleName;
					if (suffix == null)
					{
						foreach (string readerSuffix in reader.DefaultSuffix)
						{
							possibleName = name + "." + readerSuffix;
							if (Resources.TryGetValue(possibleName, out ContentInfo contentInfo))
							{
								contents.Add(contentInfo);
							}
						}
					}
					else
					{
						possibleName = name + suffix;
						if (Resources.TryGetValue(possibleName, out ContentInfo contentInfo))
						{
							contents.Add(contentInfo);
						}
					}
					if (contents.Count == 0)
					{//没有找到对应资源?
						throw new Exception("Not Found Res [" + name + "][" + type.FullName + "]");
					}
					obj = reader.Get(contents.ToArray());
				}
				if (cacheList == null) { cacheList = []; Caches.Add(key, cacheList); }
				cacheList.Add(obj);
				return obj;
			}
		}

		public static void Add(ContentInfo contentInfo)
		{
			lock (syncobj)
			{
				if (!Resources.TryGetValue(contentInfo.AbsolutePath, out ContentInfo info))
				{
					Resources.Add(contentInfo.AbsolutePath, contentInfo);
				}
				else
				{
					Resources[contentInfo.AbsolutePath] = contentInfo;
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
				if (Caches.TryGetValue(name, out var list))
				{
					var toRemove = new List<object>();
					foreach (var t in list)
					{
						if (t is IDisposable d)
						{
							d.Dispose();
						}
						toRemove.Add(t);
					}
					foreach (var t in toRemove) list.Remove(t);
				}
			}
		}

		public static bool ContainsKey(string key)
		{
			return Resources.ContainsKey(key);
		}

		public static bool IsContent(object content)
		{
			foreach (var l in Caches.Values)
			{
				foreach (var d in l) if (d == content) return true;
			}
			return false;
		}

		public static void Display_DeviceReset()
		{
			foreach (var i in Caches)
			{
				var k = i.Key;
				for (var j = 0; j < i.Value.Count; j++)
				{
					var t = i.Value[j];
					if (t is Texture2D || t is Model || t is BitmapFont)
					{
						i.Value[j] = Get(t.GetType(), k);
					}
				}
			}
		}

		public static ReadOnlyList<ContentInfo> List()
		{
			return new ReadOnlyList<ContentInfo>(Resources.Values.ToDynamicArray());
		}

		public static ReadOnlyList<ContentInfo> List(string directory)
		{
			List<ContentInfo> contents = [];
			if (!directory.EndsWith("/")) directory += "/";
			foreach (var content in Resources.Values)
			{
				if (content.ContentPath.StartsWith(directory)) contents.Add(content);
			}
			return new ReadOnlyList<ContentInfo>(contents);
		}
	}
}
