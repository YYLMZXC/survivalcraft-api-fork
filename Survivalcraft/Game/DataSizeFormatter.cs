using Engine;

namespace Game
{
	public static class DataSizeFormatter
	{
		public static string Format(long bytes, int significantDigits = 3)
		{
			if (bytes < 1024)
			{
				return $"{bytes} B";
			}
			if (bytes < 1048576)
			{
				float num = (float)bytes / 1024f;
				return string.Format(PrepareFormatString(num, "kB", significantDigits), new object[1] { num });
			}
			if (bytes < 1073741824)
			{
				float num2 = (float)bytes / 1024f / 1024f;
				return string.Format(PrepareFormatString(num2, "MB", significantDigits), new object[1] { num2 });
			}
			float num3 = (float)bytes / 1024f / 1024f / 1024f;
			return string.Format(PrepareFormatString(num3, "GB", significantDigits), new object[1] { num3 });
		}

		private static string PrepareFormatString(float value, string unit, int significantDigits)
		{
			int num = (int)(MathUtils.Log10(value) + 1f);
			return "{0:F" + MathUtils.Max(significantDigits - num, 0) + "} " + unit;
		}
	}
}
