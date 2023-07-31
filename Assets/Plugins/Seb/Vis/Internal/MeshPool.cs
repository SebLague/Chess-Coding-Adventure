using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Seb.Vis.Internal
{
	public class MeshPool
	{
		Queue<Mesh> availableMeshes;
		HashSet<Mesh> meshesInUse;

		public MeshPool()
		{
			availableMeshes = new Queue<Mesh>();
			meshesInUse = new HashSet<Mesh>();
		}

		public Mesh GetMesh()
		{
			while (availableMeshes.Count > 0)
			{
				Mesh pooledMesh = availableMeshes.Dequeue();
				if (pooledMesh != null)
				{
					meshesInUse.Add(pooledMesh);
					return pooledMesh;
				}
			}
			Mesh newMesh = new Mesh();
			meshesInUse.Add(newMesh);
			return newMesh;
		}

		public void FinishedUsingMesh(Mesh mesh)
		{
			mesh.Clear();
			availableMeshes.Enqueue(mesh);
			meshesInUse.Remove(mesh);
		}

		public void FinishedUsingAllMeshes()
		{
			foreach (Mesh mesh in meshesInUse)
			{
				availableMeshes.Enqueue(mesh);
			}
			meshesInUse.Clear();
		}
	}
}