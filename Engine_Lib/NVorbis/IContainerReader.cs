using System;

namespace NVorbis
{
	public interface IContainerReader : IDisposable
	{
		int[] StreamSerials
		{
			get;
		}

		bool CanSeek
		{
			get;
		}

		long WasteBits
		{
			get;
		}

		int PagesRead
		{
			get;
		}

		event EventHandler<NewStreamEventArgs> NewStream;

		bool Init();

		bool FindNextStream();

		int GetTotalPageCount();
	}
}
