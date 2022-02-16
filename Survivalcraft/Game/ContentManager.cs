using System;
using System.Reflection;
using System.Text;
using Engine;
using Engine.Content;
using System.IO;
namespace Game
{
	public static class ContentManager
	{
		public static void Initialize()
		{
#if android
			ContentCache.AddPackage(delegate () { return Window.Activity.Assets.Open("Content.pak"); }, Encoding.UTF8.GetBytes(Pad()), new byte[1] { 63 });
#else
			ContentCache.AddPackage(delegate() { return File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "Content.pak"); }, Encoding.UTF8.GetBytes(Pad()), new byte[1] { 63 });
#endif
		}

		public static object Get(string name)
		{
			return ContentCache.Get(name);
		}

		public static object Get(Type type, string name)
		{
			if ((object)type == typeof(Subtexture))
			{
				return TextureAtlasManager.GetSubtexture(name);
			}
			if ((object)type == typeof(string) && name.StartsWith("Strings/"))
			{
				return StringsManager.GetString(name.Substring(8));
			}
			object obj = Get(name);
			if (!type.GetTypeInfo().IsAssignableFrom(obj.GetType().GetTypeInfo()))
			{
				throw new InvalidOperationException(string.Format("Content \"{0}\" has type {1}, requested type was {2}", new object[3]
				{
					name,
					obj.GetType().FullName,
					type.FullName
				}));
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

		private static string Pad()
		{
			string text = string.Empty;
			string text2 = "0123456789abdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
			Random random = new Random(9213);
			for (int i = 0; i < 229; i++)
			{
				text += text2[random.Int(text2.Length)];
			}
			return text;
		}
	}
}
