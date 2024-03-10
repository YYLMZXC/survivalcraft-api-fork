using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#if android

using Android.App;
using static Android.App.ActivityManager;
#endif

namespace Engine
{
	public static class Utilities
	{
		public static void Swap<T>(ref T a, ref T b)
		{
			T val = a;
			a = b;
			b = val;
		}

		public static int SizeOf<T>()
		{
			return Marshal.SizeOf(typeof(T));
		}

		public static T PtrToStructure<T>(IntPtr ptr)
		{
			return (T)Marshal.PtrToStructure(ptr, typeof(T));
		}

		public static T ArrayToStructure<T>(Array array)
		{
			GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			try
			{
				return PtrToStructure<T>(gCHandle.AddrOfPinnedObject());
			}
			finally
			{
				gCHandle.Free();
			}
		}

		public static byte[] StructureToArray<T>(T structure)
		{
			byte[] array = new byte[SizeOf<T>()];
			GCHandle gCHandle = GCHandle.Alloc(structure, GCHandleType.Pinned);
			try
			{
				Marshal.Copy(gCHandle.AddrOfPinnedObject(), array, 0, array.Length);
				return array;
			}
			finally
			{
				gCHandle.Free();
			}
		}

		public static void Dispose<T>(ref T disposable) where T : IDisposable
		{
			if (disposable != null)
			{
				disposable.Dispose();
				disposable = default(T);
			}
		}

		public static void DisposeCollection<T>(ICollection<T> disposableCollection) where T : IDisposable
		{
			if (disposableCollection != null)
			{
				foreach (T item in disposableCollection)
				{
					item?.Dispose();
				}
				if (!disposableCollection.IsReadOnly)
				{
					disposableCollection.Clear();
				}
			}
		}

#if android

		public static long GetTotalAvailableMemory()
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected O, but got Unknown
			ActivityManager val = (ActivityManager)Window.Activity.GetSystemService("activity");
			MemoryInfo val2 = (MemoryInfo)(object)new MemoryInfo();
			val.GetMemoryInfo(val2);
			return val2.TotalMem;
		}

#else
		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool GlobalMemoryStatusEx(ref MEMORYINFO mi);
		//Define the information structure of memory
		[StructLayout(LayoutKind.Sequential)]
		struct MEMORYINFO
		{
			public uint dwLength; //Current structure size
			public uint dwMemoryLoad; //Current memory utilization
			public ulong ullTotalPhys; //Total physical memory size
			public ulong ullAvailPhys; //Available physical memory size
			public ulong ullTotalPageFile; //Total Exchange File Size
			public ulong ullAvailPageFile; //Total Exchange File Size
			public ulong ullTotalVirtual; //Total virtual memory size
			public ulong ullAvailVirtual; //Available virtual memory size
			public ulong ullAvailExtendedVirtual; //Keep this value always zero
		}
		private static MEMORYINFO GetMemoryStatus()
		{
			MEMORYINFO memoryInfo = new();
			memoryInfo.dwLength = (uint)Marshal.SizeOf(memoryInfo);
			GlobalMemoryStatusEx(ref memoryInfo);
			return memoryInfo;
		}
		public static double GetMemoryAvailable()
		{
			var memoryStatus = GetMemoryStatus();
			var memoryAvailable = (long)memoryStatus.ullAvailPhys;
			return memoryAvailable;
		}
		public static int GetTotalAvailableMemory() => (int)GetMemoryAvailable();
#endif
	}
}