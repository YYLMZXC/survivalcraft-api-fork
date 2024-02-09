using System;

namespace NVorbis
{
	internal class NewStreamEventArgs : EventArgs
	{
		public IPacketProvider PacketProvider
		{
			get;
			private set;
		}

		public bool IgnoreStream
		{
			get;
			set;
		}

		public NewStreamEventArgs(IPacketProvider packetProvider)
		{
			ArgumentNullException.ThrowIfNull(packetProvider);
			PacketProvider = packetProvider;
		}
	}
}
