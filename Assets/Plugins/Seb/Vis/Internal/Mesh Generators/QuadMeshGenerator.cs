using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Seb.Vis.Internal.MeshGeneration
{
	public static class QuadMeshGenerator
	{
		static Mesh cachedMesh;

		public static Mesh GetMesh()
		{
			if (cachedMesh == null)
			{
				cachedMesh = GenerateQuadMesh();
			}
			return cachedMesh;
		}

		static Mesh GenerateQuadMesh()
		{
			Mesh mesh = new Mesh();
			Vector3[] vertices = new Vector3[]
			{
				new Vector3(-0.5f, 0.5f),
				new Vector3(0.5f, 0.5f),
				new Vector3(-0.5f, -0.5f),
				new Vector3(0.5f, -0.5f)
			};
			int[] triangles = new int[] { 0, 1, 2, 2, 1, 3 };
			Vector2[] uvs = new Vector2[]
			{
				new Vector2(0,1),
				new Vector2(1,1),
				new Vector2(0,0),
				new Vector2(1,0)
			};

			mesh.SetVertices(vertices);
			mesh.SetTriangles(triangles, 0, true);
			mesh.SetUVs(0, uvs);
			//mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000);

			return mesh;
		}


	}
}