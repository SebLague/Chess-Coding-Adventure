using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Seb.Vis.Internal.MeshGeneration
{
	public static class CubeMeshGenerator
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

			var vertices = new List<Vector3>();
			var triangles = new List<int>();

			Vector3 centre = Vector3.zero;
			CreateFace(centre, Vector3.left, Vector3.forward, Vector3.up);
			CreateFace(centre, Vector3.right, -Vector3.forward, Vector3.up);
			CreateFace(centre, Vector3.forward, Vector3.right, Vector3.up);
			CreateFace(centre, -Vector3.forward, -Vector3.right, Vector3.up);
			CreateFace(centre, Vector3.up, Vector3.forward, Vector3.right);
			mesh.SetVertices(vertices);
			mesh.SetTriangles(triangles, 0, true);
			mesh.RecalculateNormals();

			return mesh;

			void CreateFace(Vector3 centre, Vector3 axisA, Vector3 axisB, Vector3 axisC)
			{
				Vector3 faceCentre = centre + axisA * 0.5f;
				Vector3 corner0 = faceCentre + (axisB + axisC) * 0.5f;
				Vector3 corner1 = corner0 + -axisB;
				Vector3 corner2 = corner1 - axisC;
				Vector3 corner3 = corner2 + axisB;

				int vertexStartIndex = vertices.Count;
				// First triangle
				vertices.Add(corner0);
				vertices.Add(corner1);
				vertices.Add(corner2);
				triangles.Add(vertexStartIndex);
				triangles.Add(vertexStartIndex + 1);
				triangles.Add(vertexStartIndex + 2);

				//Second triangle
				vertices.Add(corner2);
				vertices.Add(corner3);
				vertices.Add(corner0);
				triangles.Add(vertexStartIndex + 3);
				triangles.Add(vertexStartIndex + 4);
				triangles.Add(vertexStartIndex + 5);
			}
		}
	}
}