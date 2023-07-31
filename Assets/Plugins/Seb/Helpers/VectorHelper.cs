using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Seb
{
	public static class VectorHelper
	{
		/// <summary> Converts array of 2D vectors to array of 3D vectors with given z value (0 by default). </summary>
		public static Vector3[] To3DArray(Vector2[] array2D, float z = 0)
		{
			Vector3[] array3D = new Vector3[array2D.Length];

			for (int i = 0; i < array3D.Length; i++)
			{
				array3D[i] = new Vector3(array2D[i].x, array2D[i].y, z);
			}

			return array3D;
		}


		public static Vector3 WithX(Vector3 vec, float x)
		{
			return new Vector3(x, vec.y, vec.z);
		}

		public static Vector3 WithY(Vector3 vec, float y)
		{
			return new Vector3(vec.x, y, vec.z);
		}

		public static Vector3 WithZ(Vector3 vec, float z)
		{
			return new Vector3(vec.x, vec.y, z);
		}


	}
}