using UnityEngine;

namespace Seb
{
	public static class Maths
	{

		public const float PI = 3.1415926f;
		public const float TAU = 2 * PI;
		public const float Epsilon = 1.175494351E-38f;

		// ------ # Random Point Generation ------

		/// <summary> Random point inside of circle (uniform distribution) </summary>
		public static Vector2 RandomPointInCircle(System.Random rng)
		{
			Vector2 pointOnCircle = RandomPointInCircle(rng);
			float r = Mathf.Sqrt((float)rng.NextDouble());
			return pointOnCircle * r;
		}

		/// <summary> Random point on circumference of circle </summary>
		public static Vector2 RandomPointOnCircle(System.Random rng)
		{
			float angle = (float)rng.NextDouble() * 2 * PI;
			float x = Mathf.Cos(angle);
			float y = Mathf.Sin(angle);
			return new Vector2(x, y);
		}

		/// <summary> Random point on surface of sphere (i.e. random direction) </summary>
		public static Vector3 RandomPointOnSphere(System.Random rng)
		{
			float x = RandomNormal(rng, 0, 1);
			float y = RandomNormal(rng, 0, 1);
			float z = RandomNormal(rng, 0, 1);
			return new Vector3(x, y, z).normalized;
		}

		/// <summary> Random point inside a triangle (with uniform distribution). </summary>
		public static Vector3 RandomPointInTriangle(Vector3 a, Vector3 b, Vector3 c, System.Random rng)
		{
			double randA = rng.NextDouble();
			double randB = rng.NextDouble();
			if (randA + randB > 1)
			{
				randA = 1 - randA;
				randB = 1 - randB;
			}
			return a + (b - a) * (float)randA + (c - a) * (float)randB;
		}


		// ------ # Intersection and distance tests ------

		public static RaySphereResult RayIntersectsSphere(Vector3 rayOrigin, Vector3 rayDir, Vector3 centre, float radius)
		{
			Vector3 offset = rayOrigin - centre;
			const float a = 1;
			float b = 2 * Vector3.Dot(offset, rayDir);
			float c = Vector3.Dot(offset, offset) - radius * radius;
			float d = b * b - 4 * c; // Discriminant from quadratic formula

			// Number of intersections: 0 when d < 0; 1 when d = 0; 2 when d > 0
			if (d > 0)
			{
				float s = Mathf.Sqrt(d);
				float dstToSphereNear = Mathf.Max(0, -b - s) / (2 * a);
				float dstToSphereFar = (-b + s) / (2 * a);

				// Ignore intersections that occur behind the ray
				if (dstToSphereFar >= 0)
				{
					return new RaySphereResult()
					{
						intersects = true,
						dstToSphere = dstToSphereNear,
						dstThroughSphere = dstToSphereFar - dstToSphereNear
					};
				}
			}
			// Ray did not intersect sphere
			return new RaySphereResult()
			{
				intersects = false,
				dstToSphere = Mathf.Infinity,
				dstThroughSphere = 0
			};
		}


		/// <summary> Get point on line segment (a1, a2) that's closest to the given point (p) </summary>
		public static Vector3 ClosestPointOnLineSegment(Vector3 p, Vector3 a1, Vector3 a2)
		{
			Vector3 lineDelta = a2 - a1;
			Vector3 pointDelta = p - a1;
			float sqrLineLength = lineDelta.sqrMagnitude;

			if (sqrLineLength == 0)
				return a1;

			float t = Mathf.Clamp01(Vector3.Dot(pointDelta, lineDelta) / sqrLineLength);
			return a1 + lineDelta * t;
		}

		/// <summary> Calculates smallest distance from given point to the line segment (a1, a2)</summary>
		public static float DistanceToLineSegment(Vector3 p, Vector3 a1, Vector3 a2)
		{
			Vector3 closestPoint = ClosestPointOnLineSegment(p, a1, a2);
			return (p - closestPoint).magnitude;
		}

		/// <summary> Test if two infinite 2D lines intersect (don't if parallel), and get point of intersection </summary>
		public static (bool intersects, Vector2 point) LineIntersectsLine(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
		{
			float d = (a1.x - a2.x) * (b1.y - b2.y) - (a1.y - a2.y) * (b1.x - b2.x);
			// Check if parallel
			if (ApproximatelyEqual(d, 0))
			{
				return (false, Vector2.zero);
			}

			float n = (a1.x - b1.x) * (b1.y - b2.y) - (a1.y - b1.y) * (b1.x - b2.x);
			float t = n / d;
			Vector2 intersectionPoint = a1 + (a2 - a1) * t;
			return (true, intersectionPoint);
		}

		/// <summary> Test if ray intersects infinite line, and get point of intersection </summary>
		public static (bool intersects, Vector2 point) RayIntersectsLine(Vector2 rayOrigin, Vector2 rayDir, Vector2 b1, Vector2 b2)
		{
			Vector2 a1 = rayOrigin;
			Vector2 a2 = a1 + rayDir;
			float d = (a1.x - a2.x) * (b1.y - b2.y) - (a1.y - a2.y) * (b1.x - b2.x);
			// Check if parallel
			if (ApproximatelyEqual(d, 0))
			{
				return (false, Vector2.zero);
			}

			float n = (a1.x - b1.x) * (b1.y - b2.y) - (a1.y - b1.y) * (b1.x - b2.x);
			float t = n / d;
			Vector2 intersectionPoint = rayOrigin + rayDir * t;
			bool intersectsInFrontOfRay = t >= 0;
			return (intersectsInFrontOfRay, intersectionPoint);
		}

		/// <summary> Returns -1 or +1 depending which side point p is of the line (a1, a2). Returns 0 if on line. </summary>
		public static int SideOfLine(Vector2 p, Vector2 a, Vector2 b)
		{
			float r = (b.x - a.x) * (p.y - a.y) - (p.x - a.x) * (b.y - a.y);
			return System.Math.Sign(r);
		}

		/// <summary> Test if points p1 and p2 are on the same side of the line (a1, a2) </summary>
		public static bool PointOnSameSideOfLine(Vector2 p1, Vector2 p2, Vector2 a1, Vector2 a2)
		{
			return SideOfLine(p1, a1, a2) == SideOfLine(p2, a1, a2);
		}

		// ------ # Miscellaneous ------

		/// <summary> Returns the area of a triangle in 3D space. </summary>
		public static float TriangleArea(Vector3 a, Vector3 b, Vector3 c)
		{
			// Thanks to https://math.stackexchange.com/a/1951650
			Vector3 ortho = Vector3.Cross(c - a, b - a);
			float parallogramArea = ortho.magnitude;
			return parallogramArea * 0.5f;
		}

		/// <summary>
		/// Returns the signed area of a triangle in 2D space.
		/// The sign depends on the whether the points are given in clockwise (negative) or counter-clockwise (positive) order.
		/// </summary>
		public static float TriangleAreaSigned2D(Vector2 a, Vector2 b, Vector2 c)
		{
			return 0.5f * (-b.y * c.x + a.y * (-b.x + c.x) + a.x * (b.y - c.y) + b.x * c.y);
		}

		/// <summary> Test if point p is inside the triangle (a, b, c) </summary>
		public static bool TriangleContainsPoint(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
		{
			// Thanks to https://stackoverflow.com/a/14382692
			float area = TriangleAreaSigned2D(a, b, c);
			float s = (a.y * c.x - a.x * c.y + (c.y - a.y) * p.x + (a.x - c.x) * p.y) * Mathf.Sign(area);
			float t = (a.x * b.y - a.y * b.x + (a.y - b.y) * p.x + (b.x - a.x) * p.y) * Mathf.Sign(area);
			return s >= 0 && t >= 0 && s + t < 2 * Mathf.Abs(area);
		}

		/// <summary> Determines whether the given 2D triangle is wound in a clockwise order</summary>

		public static bool TriangleIsClockwise(Vector2 a, Vector2 b, Vector2 c)
		{
			return TriangleAreaSigned2D(a, b, c) < 0;
		}


		/// <summary>
		/// Returns a random value with normal distribution.
		/// The mean determines the 'centre' of the distribution. 
		/// The standardDeviation controls the spread of the distribution (i.e. how likely it is to get values that are far from the mean).
		/// See https://www.desmos.com/calculator/0dnzmd0x0h for example.
		/// </summary>
		public static float RandomNormal(System.Random rng, float mean = 0, float standardDeviation = 1)
		{
			// Thanks to https://stackoverflow.com/a/6178290
			float theta = 2 * Mathf.PI * (float)rng.NextDouble();
			float rho = Mathf.Sqrt(-2 * Mathf.Log((float)rng.NextDouble()));
			float scale = standardDeviation * rho;
			return mean + scale * Mathf.Cos(theta);
		}

		/// <summary>
		/// Pick random index, weighted by the weights array.
		/// For example, if the array contains {1, 6, 3}...
		/// The possible indices would be (0, 1, 2)
		/// and the probabilities for these would be (1/10, 6/10, 3/10)
		/// </summary>
		public static int WeightedRandomIndex(System.Random prng, float[] weights)
		{
			float weightSum = 0;
			for (int i = 0; i < weights.Length; i++)
			{
				weightSum += weights[i];
			}

			float randomValue = (float)prng.NextDouble() * weightSum;
			float cumul = 0;

			for (int i = 0; i < weights.Length; i++)
			{
				cumul += weights[i];
				if (randomValue < cumul)
				{
					return i;
				}
			}

			return weights.Length - 1;
		}

		/// <summary>
		/// Returns the length of the shortest arc between two points on the surface of a unit sphere.
		/// </summary>
		public static float ArcLengthBetweenPointsOnUnitSphere(Vector3 a, Vector3 b)
		{
			// Thanks to https://www.movable-type.co.uk/scripts/latlong-vectors.html
			return Mathf.Atan2(Vector3.Cross(a, b).magnitude, Vector3.Dot(a, b));
			// Note: The following (simpler) approach works too, but is less precise for small angles:
			// return Mathf.Acos(Vector3.Dot(a, b));
		}

		/// <summary>
		/// Returns the length of the shortest arc between two points on the surface of a sphere with the specified radius.
		/// </summary>
		public static float ArcLengthBetweenPointsOnSphere(Vector3 a, Vector3 b, float sphereRadius)
		{
			return ArcLengthBetweenPointsOnUnitSphere(a.normalized, b.normalized) * sphereRadius;
		}

		/// <summary>
		/// Returns n points distributed reasonably evenly on a sphere.
		/// Uses fibonacci spiral technique.
		/// </summary>
		public static Vector3[] GetPointsOnSphereSurface(int numPoints, float radius = 1)
		{
			// Thanks to https://stackoverflow.com/questions/9600801/evenly-distributing-n-points-on-a-sphere/44164075#44164075
			Vector3[] points = new Vector3[numPoints];
			const double goldenRatio = 1.618033988749894; // (1 + sqrt(5)) / 2
			const double angleIncrement = System.Math.PI * 2 * goldenRatio;

			System.Threading.Tasks.Parallel.For(0, numPoints, i =>
			{
				double t = (double)i / numPoints;
				double inclination = System.Math.Acos(1 - 2 * t);
				double azimuth = angleIncrement * i;

				double x = System.Math.Sin(inclination) * System.Math.Cos(azimuth);
				double y = System.Math.Sin(inclination) * System.Math.Sin(azimuth);
				double z = System.Math.Cos(inclination);
				points[i] = new Vector3((float)x, (float)y, (float)z) * radius;
			});
			return points;
		}

		public static (Vector2 centre, Vector2 size) BoundingBox(Vector2[] points)
		{
			if (points.Length == 0)
			{
				return (Vector2.zero, Vector2.zero);
			}

			Vector2 min = points[0];
			Vector2 max = points[0];
			for (int i = 1; i < points.Length; i++)
			{
				Vector2 p = points[i];
				min = new Vector2(Min(min.x, p.x), Min(min.y, p.y));
				max = new Vector2(Max(max.x, p.x), Max(max.y, p.y));
			}

			Vector2 centre = (min + max) / 2;
			Vector2 size = max - min;
			return (centre, size);
		}



		public static bool ApproximatelyEqual(float a, float b) => System.Math.Abs(a - b) < Epsilon;
		public static float Min(float a, float b) => a < b ? a : b;
		public static float Max(float a, float b) => a > b ? a : b;
		
		public struct RaySphereResult
		{
			public bool intersects;
			public float dstToSphere;
			public float dstThroughSphere;
		}
	}
}