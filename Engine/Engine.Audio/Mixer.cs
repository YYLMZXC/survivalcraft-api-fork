using System.Reflection;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

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
			//直接加载
			string environmentVariable = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
			string fullPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location == ""? AppContext.BaseDirectory: Assembly.GetExecutingAssembly().Location);//路径备选方案
			Environment.SetEnvironmentVariable("PATH", fullPath + ";" + environmentVariable, EnvironmentVariableTarget.Process);
			//释放文件
			new AudioContext();
			if(CheckALError())
			{
				string dllName = "openal32.dll"; // DLL资源名称
				string ALPath = Path.Combine(fullPath, dllName);
				if (!File.Exists(ALPath))//检测外置dll是否存在，如果不存在就释放
				{
					using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(dllName))
					using (FileStream fileStream = new(ALPath, FileMode.Create))
					{
						stream.CopyTo(fileStream);
					}
				}
				//Assembly dllAssembly = Assembly.LoadFile(ALPath);
				new AudioContext();
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
			public static bool CheckALError()//注意返回值为是否出错
		{
			try
			{
				ALError error = AL.GetError();
				if (error != ALError.NoError)
				{
					Log.Error("OPENAL出错!");
					Log.Error(error);
					//throw new InvalidOperationException(AL.GetErrorString(error));
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (Exception e)
			{
				Log.Error("OPENAL无法调用");
				Log.Error (e);
				return true;
			}
		}
	}
}
