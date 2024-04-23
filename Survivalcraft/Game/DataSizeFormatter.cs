using Engine;

namespace Game
{
	public static class DataSizeFormatter
	{
		public static string Format(long bytes)
		{
			if (bytes < 1024)
			{
				return $"{bytes}B";
			}
			if (bytes < 1048576)
			{
				float num = bytes / 1024f;
				return string.Format(PrepareFormatString(num, "kB"), num);
			}
			if (bytes < 1073741824)
			{
				float num2 = bytes / 1024f / 1024f;
				return string.Format(PrepareFormatString(num2, "MB"), num2);
			}
			float num3 = bytes / 1024f / 1024f / 1024f;
			return string.Format(PrepareFormatString(num3, "GB"), num3);
		}

		public static string PrepareFormatString(float value, string unit)
		{
			int num = (int)(MathF.Log10(value) + 1f);
			return "{0:F" + Math.Max(3 - num, 0).ToString() + "}" + unit;
		}
	}
}
