﻿using Engine;
using Game;

namespace Survivalcraft.WindowsLauncher
{
	public class DiskExternalContentProvider : IExternalContentProvider
	{
		public string DisplayName => LanguageControl.Get(fName, "DisplayName");

		public bool SupportsLinks => false;

		public bool SupportsListing => true;

		public bool RequiresLogin => false;

		public bool IsLoggedIn => true;

		public static string fName = "DiskExternalContentProvider";

		public static string LocalPath = ModsManager.ExtPath;

		public string Description => LanguageControl.Get(fName, "Description");

		public DiskExternalContentProvider()
		{
			if (!Storage.DirectoryExists(LocalPath))
			{
				Storage.CreateDirectory(LocalPath);
			}
		}
		public void Dispose()
		{
		}

		public void Download(string path, CancellableProgress progress, Action<Stream> success, Action<Exception> failure)
		{
			FileStream fileStream = null;
			if (!File.Exists(path))
			{
				failure(new FileNotFoundException());
				return;
			}

			fileStream = File.OpenRead(path);
			Exception e = default;
			ThreadPool.QueueUserWorkItem(delegate
			{
				try
				{
					success(fileStream);
				}
				catch (Exception ex)
				{
					failure(ex);
				}
			});
		}

		public void Link(string path, CancellableProgress progress, Action<string> success, Action<Exception> failure)
		{
			failure(new NotSupportedException());
		}

		public void List(string path, CancellableProgress progress, Action<ExternalContentEntry> success, Action<Exception> failure)
		{
			ExternalContentEntry entry = default;
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
			Exception e = default;
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
			ThreadPool.QueueUserWorkItem(delegate
			{
				try
				{
					string internalPath = path;
					entry = GetDirectoryEntry(internalPath, scanContents: true);
					success(entry);
				}
				catch (Exception ex)
				{
					failure(ex);
				}
			});
		}

		public void Login(CancellableProgress progress, Action success, Action<Exception> failure)
		{
			failure(new NotSupportedException());
		}

		public void Logout()
		{
			throw new NotSupportedException();
		}

		public void Upload(string path, Stream stream, CancellableProgress progress, Action<string> success, Action<Exception> failure)
		{
			//var saveFileDialog1 = new SaveFileDialog();
			try
			{
				FileStream fileStream = null;
				string nPath = Path.Combine(LocalPath, path);
				fileStream = !File.Exists(nPath) ? File.Create(nPath) : File.OpenWrite(nPath);
				stream.CopyTo(fileStream);
				fileStream.Close();
				success(null);
			}
			catch (Exception e)
			{
				failure(e);
			}
		}
		private ExternalContentEntry GetDirectoryEntry(string internalPath, bool scanContents)
		{
			var externalContentEntry = new ExternalContentEntry();
			externalContentEntry.Type = ExternalContentType.Directory;
			externalContentEntry.Path = internalPath;
			externalContentEntry.Time = new DateTime(1970, 1, 1);
			if (scanContents)
			{
				string[] directories = Directory.GetDirectories(internalPath);
				foreach (string internalPath2 in directories)
				{
					externalContentEntry.ChildEntries.Add(GetDirectoryEntry(internalPath2, scanContents: false));
				}
				directories = Directory.GetFiles(internalPath);
				foreach (string text in directories)
				{
					var fileInfo = new FileInfo(text);
					var externalContentEntry2 = new ExternalContentEntry();
					externalContentEntry2.Type = ExternalContentManager.ExtensionToType(Path.GetExtension(text));
					externalContentEntry2.Path = text;
					externalContentEntry2.Size = fileInfo.Length;
					externalContentEntry2.Time = fileInfo.CreationTime;
					externalContentEntry.ChildEntries.Add(externalContentEntry2);
				}
			}
			return externalContentEntry;
		}

	}
}
