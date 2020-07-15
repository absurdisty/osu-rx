using System;
using System.Numerics;

namespace osu_rx.Helpers
{
	public class MathHelper
	{
		public static int Clamp(int value, int min, int maX)
		{
			if (value < min)
			{
				return min;
			}
			if (value <= maX)
			{
				return value;
			}
			return maX;
		}

		public static double GetAngle(Vector2 a, Vector2 b, Vector2 c)
		{
			float num = Vector2.DistanceSquared(a, b);
			float num2 = Vector2.DistanceSquared(b, c);
			float num3 = Vector2.DistanceSquared(a, c);
			double num4 = Math.Acos((double)(num + num3 - num2) / (2.0 * Math.Sqrt((double)num) * Math.Sqrt((double)num3)));
			if (num4 < 0.0)
			{
				num4 = 3.1415926535897931 - num4;
			}
			return num4 * MathHelper.RadToDeg;
		}

		public static double RadToDeg
		{
			get
			{
				return 57.295779513082323;
			}
		}
	}
}
