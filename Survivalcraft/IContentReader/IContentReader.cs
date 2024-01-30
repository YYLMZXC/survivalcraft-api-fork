namespace Game.IContentReader
{
	public interface IContentReader
	{
		public string Type { get; }
		public string[] DefaultSuffix { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="contents"></param>
		/// <returns></returns>
		public object Get(ContentInfo[] contents);
	}
}
