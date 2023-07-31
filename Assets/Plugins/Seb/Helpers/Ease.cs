namespace Seb
{
	// All functions take a time value 't' between 0 and 1 and return the eased result.
	// Ease In: starts slow and accelerates
	// Ease Out: starts fast and decelerates
	// Ease InOut: starts slow, speeds up in the middle, and slows down at the end

	// Some functions thanks to www.easings.net

	public static class Ease
	{

		public enum EaseType
		{
			Linear,
			Quadratic,
			Cubic
		}

		public enum EaseDirection
		{
			InOut,
			In,
			Out
		}

		public static float GetEasing(float t, EaseType type, EaseDirection direction) => type switch
		{
			EaseType.Linear => Linear.GetValue(t),
			EaseType.Quadratic => Quadratic.GetValue(t, direction),
			EaseType.Cubic => Cubic.GetValue(t, direction),
			_ => 0
		};

		// Linear easing (i.e. no easing)
		public static class Linear
		{
			public static float GetValue(float t) => Clamp01(t);
		}

		// Quadratic easing
		public static class Quadratic
		{
			public static float GetValue(float t, EaseDirection direction)
			{
				switch (direction)
				{
					case EaseDirection.In: return In(t);
					case EaseDirection.Out: return Out(t);
					case EaseDirection.InOut: return InOut(t);
					default: return 0;
				}
			}

			public static float In(float t)
			{
				t = Clamp01(t);
				return t * t;
			}
			public static float Out(float t)
			{
				t = Clamp01(t);
				return 1 - Square(1 - t);
			}

			public static float InOut(float t)
			{
				t = Clamp01(t);
				return 3 * Square(t) - 2 * Cube(t);
			}
		}

		// Cubic easing
		public static class Cubic
		{
			public static float GetValue(float t, EaseDirection direction = EaseDirection.InOut)
			{
				switch (direction)
				{
					case EaseDirection.In: return In(t);
					case EaseDirection.Out: return Out(t);
					case EaseDirection.InOut: return InOut(t);
					default: return 0;
				}
			}

			public static float In(float t)
			{
				t = Clamp01(t);
				return t * t * t;
			}

			public static float Out(float t)
			{
				t = Clamp01(t);
				return 1 - Cube(1 - t);
			}

			public static float InOut(float t)
			{
				t = Clamp01(t);
				float a = 4 * Cube(t);
				float b = 1 - Cube(-2 * t + 2) / 2;
				return t < 0.5f ? a : b;
			}

		}

		public static class Asymmetric
		{
			public static float QuadInCubeOut(float t)
			{
				float blend = Cubic.InOut(t);
				return Lerp(Quadratic.In(t), Cubic.Out(t), blend);
			}

			public static float CubeInQuadOut(float t)
			{
				float blend = Cubic.InOut(t);
				return Lerp(Cubic.In(t), Quadratic.Out(t), blend);
			}
		}

		static float Clamp01(float t) => System.Math.Clamp(t, 0, 1);
		static float Square(float x) => x * x;
		static float Cube(float x) => x * x * x;
		static float Lerp(float a, float b, float t) => a * (1 - Clamp01(t)) + b * Clamp01(t);

	}
}