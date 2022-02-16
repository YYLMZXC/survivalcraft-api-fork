using SharpDX.XAudio2;

namespace Engine.Audio
{
	public static class Mixer
	{
		private static float m_masterVolume = 1f;

		internal static XAudio2 m_xAudio2;

		internal static MasteringVoice m_masteringVoice;

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
			m_xAudio2 = new XAudio2(XAudio2Flags.None, ProcessorSpecifier.AnyProcessor);
			m_masteringVoice = new MasteringVoice(m_xAudio2);
		}

		internal static void Dispose()
		{
			m_masteringVoice.Dispose();
		}

		internal static void BeforeFrame()
		{
		}

		internal static void AfterFrame()
		{
		}

		internal static void InternalSetMasterVolume(float volume)
		{
			m_masteringVoice.SetVolume(m_masterVolume);
		}
	}
}
