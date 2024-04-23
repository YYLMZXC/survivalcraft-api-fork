namespace Engine
{
	public static class MathUtils
	{
		public const float PI = MathF.PI;

		public const float E = MathF.E;

		public static int Min(int x1, int x2)
		{
			return Math.Min(x1, x2);
		}

		public static int Min(int x1, int x2, int x3)
		{
			return Min(Min(x1, x2), x3);
		}

		public static int Min(int x1, int x2, int x3, int x4)
		{
			return Min(Min(Min(x1, x2), x3), x4);
		}

		public static int Max(int x1, int x2)
		{
			return Math.Max(x1, x2);
		}

		public static int Max(int x1, int x2, int x3)
		{
			return Max(Max(x1, x2), x3);
		}

		public static int Max(int x1, int x2, int x3, int x4)
		{
			return Max(Max(Max(x1, x2), x3), x4);
		}

		public static int Clamp(int x, int min, int max)
		{
			return Math.Clamp(x, min, max);
		}

		public static int Abs(int x)
		{
			return Math.Abs(x);
		}

		public static int Sqr(int x)
		{
			return x * x;
		}

		public static bool IsPowerOf2(uint x)
		{
			if (x != 0)
			{
				return (x & (x - 1)) == 0;
			}
			return false;
		}

		public static uint NextPowerOf2(uint x)
		{
			x--;
			x |= x >> 1;
			x |= x >> 2;
			x |= x >> 4;
			x |= x >> 8;
			x |= x >> 16;
			x++;
			return x;
		}

		public static uint Hash(uint key)
		{
			key ^= key >> 16;
			key *= 2146121005;
			key ^= key >> 15;
			key = (uint)((int)key * -2073254261);
			key ^= key >> 16;
			return key;
		}

		public static long Min(long x1, long x2)
		{
			return Math.Min(x1, x2);
		}

		public static long Min(long x1, long x2, long x3)
		{
			return Min(Min(x1, x2), x3);
		}

		public static long Min(long x1, long x2, long x3, long x4)
		{
			return Min(Min(Min(x1, x2), x3), x4);
		}

		public static long Max(long x1, long x2)
		{
			return Math.Max(x1, x2);
		}

		public static long Max(long x1, long x2, long x3)
		{
			return Max(Max(x1, x2), x3);
		}

		public static long Max(long x1, long x2, long x3, long x4)
		{
			return Max(Max(Max(x1, x2), x3), x4);
		}

		public static long Sqr(long x)
		{
			return x * x;
		}

		public static bool IsPowerOf2(long x)
		{
			if (x > 0)
			{
				return (x & (x - 1)) == 0;
			}
			return false;
		}

		public static ulong NextPowerOf2(ulong x)
		{
			x--;
			x |= x >> 1;
			x |= x >> 2;
			x |= x >> 4;
			x |= x >> 8;
			x |= x >> 16;
			x |= x >> 32;
			x++;
			return x;
		}

		public static float Min(float x1, float x2)
		{
			return MathF.Min(x1, x2);
		}

		public static float Min(float x1, float x2, float x3)
		{
			return Min(Min(x1, x2), x3);
		}

		public static float Min(float x1, float x2, float x3, float x4)
		{
			return Min(Min(Min(x1, x2), x3), x4);
		}

		public static float Max(float x1, float x2)
		{
			return MathF.Max(x1, x2);
		}

		public static float Max(float x1, float x2, float x3)
		{
			return Max(Max(x1, x2), x3);
		}

		public static float Max(float x1, float x2, float x3, float x4)
		{
			return Max(Max(Max(x1, x2), x3), x4);
		}

		public static float Clamp(float x, float min, float max)
		{
			return Math.Clamp(x, min, max);
		}

		public static float Saturate(float x)
		{
			if (!(x < 0f))
			{
				if (!(x > 1f))
				{
					return x;
				}
				return 1f;
			}
			return 0f;
		}

		public static float Sign(float x)
		{
			return MathF.Sign(x);
		}

		public static float Abs(float x)
		{
			return MathF.Abs(x);
		}

		public static float Floor(float x)
		{
			return MathF.Floor(x);
		}

		public static float Ceiling(float x)
		{
			return MathF.Ceiling(x);
		}

		public static float Round(float x)
		{
			return MathF.Round(x);
		}

		public static float Remainder(float x, float y)
		{
			return x - (Floor(x / y) * y);
		}

		public static float Sqr(float x)
		{
			return x * x;
		}
		public static float Atan2(float y, float x)
		{
			return MathF.Atan2(y, x);
		}
		public static float PowSign(float x, float n)
		{
			return Sign(x) * MathF.Pow(MathF.Abs(x), n);
		}

		public static float Lerp(float x1, float x2, float f)
		{
			return x1 + ((x2 - x1) * f);
		}

		public static float SmoothStep(float min, float max, float x)
		{
			x = Clamp((x - min) / (max - min), 0f, 1f);
			return x * x * (3f - (2f * x));
		}

		public static float CatmullRom(float v1, float v2, float v3, float v4, float f)
		{
			float num = f * f;
			float num2 = num * f;
			return 0.5f * ((2f * v2) + ((v3 - v1) * f) + (((2f * v1) - (5f * v2) + (4f * v3) - v4) * num) + (((3f * v2) - v1 - (3f * v3) + v4) * num2));
		}

		public static float NormalizeAngle(float angle)
		{
			angle = (float)Math.IEEERemainder(angle, 6.2831854820251465);
			if (angle > (float)Math.PI)
			{
				angle -= (float)Math.PI * 2f;
			}
			else if (angle <= -(float)Math.PI)
			{
				angle += (float)Math.PI * 2f;
			}
			return angle;
		}

		public static float Sigmoid(float x, float steepness)
		{
			if (x <= 0f)
			{
				return 0f;
			}
			if (x >= 1f)
			{
				return 1f;
			}
			float num = MathF.Exp(steepness);
			float num2 = MathF.Exp(2f * steepness * x);
			return num * (num2 - 1f) / ((num - 1f) * (num2 + num));
		}

		public static float DegToRad(float degrees)
		{
			return degrees / 180f * (float)Math.PI;
		}

		public static float RadToDeg(float radians)
		{
			return radians * 180f / (float)Math.PI;
		}

		public static double Min(double x1, double x2)
		{
			return Math.Min(x1, x2);
		}

		public static double Max(double x1, double x2)
		{
			return Math.Max(x1, x2);
		}

		public static double Saturate(double x)
		{
			if (!(x < 0.0))
			{
				if (!(x > 1.0))
				{
					return x;
				}
				return 1.0;
			}
			return 0.0;
		}

		public static double Floor(double x)
		{
			return Math.Floor(x);
		}

		public static double Round(double x)
		{
			return Math.Round(x);
		}

		public static double Remainder(double x, double y)
		{
			return x - (Floor(x / y) * y);
		}

		public static double PowSign(double x, double n)
		{
			return Math.Sign(x) * Math.Pow(Math.Abs(x), n);
		}

		public static double Lerp(double x1, double x2, double f)
		{
			return x1 + ((x2 - x1) * f);
		}

		public static double SmoothStep(double min, double max, double x)
		{
			x = Math.Clamp((x - min) / (max - min), 0.0, 1.0);
			return x * x * (3.0 - (2.0 * x));
		}
		
		public static double CatmullRom(double v1, double v2, double v3, double v4, double f)
		{
			double num = f * f;
			double num2 = num * f;
			return 0.5 * ((2.0 * v2) + ((v3 - v1) * f) + (((2.0 * v1) - (5.0 * v2) + (4.0 * v3) - v4) * num) + (((3.0 * v2) - v1 - (3.0 * v3) + v4) * num2));
		}

		public static double NormalizeAngle(double angle)
		{
			angle = Math.IEEERemainder(angle, Math.PI * 2.0);
			if (angle > 3.1415927410125732)
			{
				angle -= Math.PI * 2.0;
			}
			else if (angle <= -Math.PI)
			{
				angle += Math.PI * 2.0;
			}
			return angle;
		}

		public static double DegToRad(double degrees)
		{
			return degrees / 180.0 * Math.PI;
		}

		public static double RadToDeg(double radians)
		{
			return radians * 180.0 / Math.PI;
		}

		public static float LinearStep(float zero, float one, float f)
		{
			return Saturate((f - zero) / (one - zero));
		}
	}
}