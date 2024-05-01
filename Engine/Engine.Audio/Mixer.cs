using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Engine.Audio
{
	public static class Mixer
	{
		private static float m_masterVolume = 1f;

        public static readonly List<Sound> m_soundsToStop = [];

        public static HashSet<Sound> m_soundsToStopPoll = [];

		public static float MasterVolume
		{
			get
			{
				return m_masterVolume;
			}
			set
			{
				value = MathUtils.Saturate(value);
				if (value != m_masterVolume)
				{
					m_masterVolume = value;
					InternalSetMasterVolume(value);
				}
			}
		}
		internal static void Initialize()
		{
			
#if desktop
			//零次准备，系统安装了openal
			//一次准备，让opentk加载openal32.dll
			string environmentVariable = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
			string fullPath = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
			string str = Path.Combine(fullPath);
			Environment.SetEnvironmentVariable("PATH", str + ";" + environmentVariable, EnvironmentVariableTarget.Process);
			//二次准备，检测外置文件并主动加载
			new AudioContext();
			string ALPath = "app:/openal32.dll";
			if (Storage.FileExists(ALPath) && CheckALError())//检测外置资源是否存在，如果不存在就使用内置资源
			{
				Assembly dllAssembly = Assembly.LoadFile(ALPath);
			}
			else
			{
				// 提取嵌入的DLL到临时文件夹
				string tempPath = Path.GetTempFileName();
				File.Delete(tempPath); // 删除临时文件，确保路径可用
				Directory.CreateDirectory(tempPath);
				string dllName = "openal32.dll"; // DLL资源名称
				string tempDllPath = Path.Combine(tempPath, dllName);

				using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(dllName))
				using (FileStream fileStream = new(tempDllPath, FileMode.Create))
				{
					stream.CopyTo(fileStream);
				}

				// 加载DLL并调用其中的方法
				Assembly dllAssembly = Assembly.LoadFile(tempDllPath);
				// ... 使用反射调用DLL中的方法 ...

				// 清理临时文件
				Directory.Delete(tempPath, true);
			}
#else
			new AudioContext();
			CheckALError();
#endif


		}
		internal static void Dispose()
		{
		}

		internal static void BeforeFrame()
		{
			foreach (Sound item in m_soundsToStopPoll)
			{
				if (item.m_source != 0 && item.State == SoundState.Playing && AL.GetSourceState(item.m_source) == ALSourceState.Stopped)
				{
					m_soundsToStop.Add(item);
				}
			}
			foreach (Sound item2 in m_soundsToStop)
			{
				item2.Stop();
			}
			m_soundsToStop.Clear();
		}

		internal static void AfterFrame()
		{
		}

		internal static void InternalSetMasterVolume(float volume)
		{
			AL.Listener(ALListenerf.Gain, volume);
		}
		/*
		internal static void CheckALError()
		{
			ALError error = AL.GetError();
			//if (error != 0)
			//{
			//	throw new InvalidOperationException(AL.GetErrorString(error));
			//}
		}*/
			public static bool CheckALError()
		{
			try
			{
				ALError error = AL.GetError();
				if (error != 0)
				{
					Log.Error("OPENAL出错!");
					Log.Error(AL.GetError());
					//throw new InvalidOperationException(AL.GetErrorString(error));
					return true;//返回是否出错
				}
				else
				{
					return false;
				}
			}
			catch (Exception e)
			{
				Log.Error("OPENAL疑似未安装!");
				Log.Error (e);
				return true;
			}
		}
	}
}
