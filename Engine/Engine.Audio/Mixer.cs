using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;

namespace Engine.Audio
{
	public static class Mixer
	{
		private static float m_masterVolume = 1f;

		private static readonly List<Sound> m_soundsToStop = [];

		internal static HashSet<Sound> m_soundsToStopPoll = [];

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
			//string environmentVariable = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
			//string fullPath = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
			//string path = Environment.Is64BitProcess ? "OpenAL" : "OpenAL86";
			//string str = Path.Combine(fullPath,  path);
			//Environment.SetEnvironmentVariable("PATH", str + ";" + environmentVariable, EnvironmentVariableTarget.Process);
#endif
			CheckALError();
			new AudioContext();
			
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
