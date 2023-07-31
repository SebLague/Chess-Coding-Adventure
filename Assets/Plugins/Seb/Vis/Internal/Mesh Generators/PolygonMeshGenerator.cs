using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Seb.Vis.Internal.MeshGeneration
{
	public static class PolygonMeshGenerator
	{

		public static void GeneratePolygonMesh(Mesh mesh, Vector2[] points)
		{
			mesh.Clear();
			(Vector2[] verts2D, int[] indices) = Triangulation.Triangulator.Triangulate(points);
			Vector3[] verts = To3DArray(verts2D, 0);

			mesh.SetVertices(verts);
			mesh.SetTriangles(indices, 0, true);
		}

		static Vector3[] To3DArray(Vector2[] array2D, float z = 0)
		{
			Vector3[] array3D = new Vector3[array2D.Length];

			for (int i = 0; i < array3D.Length; i++)
			{
				array3D[i] = new Vector3(array2D[i].x, array2D[i].y, z);
			}

			return array3D;
		}


	}
}