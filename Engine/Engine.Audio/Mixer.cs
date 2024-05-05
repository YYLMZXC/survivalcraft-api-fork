using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

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
			//���׼����ϵͳ��װ��openal
			//һ��׼������opentk����openal32.dll
			string environmentVariable = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
			string fullPath = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
			string str = Path.Combine(fullPath);
			Environment.SetEnvironmentVariable("PATH", str + ";" + environmentVariable, EnvironmentVariableTarget.Process);
			//����׼������������ļ�����������
			string dllName = "openal32.dll"; // DLL��Դ����
			string ALPath = Path.Combine(fullPath,dllName);
			new AudioContext();
			if (Storage.FileExists(ALPath) && CheckALError())//���������Դ�Ƿ���ڣ���������ھ�ʹ��������Դ
			{
				Assembly dllAssembly = Assembly.LoadFile(ALPath);
			}
			else
			{
				using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(dllName))
				using (FileStream fileStream = new(ALPath, FileMode.Create))
				{
					stream.CopyTo(fileStream);
				}
				// ����DLL���������еķ���
				Assembly dllAssembly = Assembly.LoadFile(ALPath);
				// ... ʹ�÷������DLL�еķ��� ...
			}
			new AudioContext();
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
					Log.Error("OPENAL����!");
					Log.Error(AL.GetError());
					//throw new InvalidOperationException(AL.GetErrorString(error));
					return true;//�����Ƿ����
				}
				else
				{
					return false;
				}
			}
			catch (Exception e)
			{
				Log.Error("OPENAL����δ��װ!");
				Log.Error (e);
				return true;
			}
		}
	}
}
