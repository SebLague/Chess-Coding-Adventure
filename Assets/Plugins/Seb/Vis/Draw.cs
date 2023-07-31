using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Seb.Vis.Internal;
using Seb.Vis.Internal.MeshGeneration;
using UnityEngine.Rendering;

namespace Seb.Vis
{
	public static class Draw
	{

		static MaterialPropertyBlock materialProperties;
		static CommandBuffer cmd;
		static int lastFrameWithDrawCommands;
		static MeshPool meshPool;
		const string buffName = "Vis Draw Commands";

		public static void Polygon(Vector2[] points, Color col)
		{
			if (points.Length <= 2) { return; }

			PerDrawInit();
			materialProperties.SetColor(DrawMaterials.colorID, col);
			Mesh mesh = meshPool.GetMesh();
			PolygonMeshGenerator.GeneratePolygonMesh(mesh, points);
			cmd.DrawMesh(mesh, Matrix4x4.identity, DrawMaterials.unlitMat, 0, 0, materialProperties);
		}


		public static void Quad(Vector2 centre, Vector2 size, Color col)
		{
			if (size.x == 0 && size.y == 0) { return; }
			PerDrawInit();
			materialProperties.SetColor(DrawMaterials.colorID, col);
			Matrix4x4 matrix = Matrix4x4.TRS(centre, Quaternion.identity, new Vector3(size.x, size.y, 1));
			cmd.DrawMesh(QuadMeshGenerator.GetMesh(), matrix, DrawMaterials.unlitMat, 0, 0, materialProperties);
		}

		public static void Quad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color col)
		{
			PerDrawInit();
			Mesh mesh = QuadMeshGenerator.GetMesh();
			materialProperties.SetColor(DrawMaterials.colorID, col);
			materialProperties.SetVector(DrawMaterials.quadPointA, a);
			materialProperties.SetVector(DrawMaterials.quadPointB, b);
			materialProperties.SetVector(DrawMaterials.quadPointC, c);
			materialProperties.SetVector(DrawMaterials.quadPointD, d);
			cmd.DrawMesh(mesh, Matrix4x4.identity, DrawMaterials.quadMat, 0, 0, materialProperties);
		}

		public static void Mesh(Mesh mesh, Color col)
		{
			Mesh(mesh, Vector3.zero, Quaternion.identity, Vector3.one, col);
		}

		public static void Mesh(Mesh mesh, Vector3 pos, Quaternion rot, Vector3 scale, Color col)
		{
			PerDrawInit();
			materialProperties.SetColor(DrawMaterials.colorID, col);
			Matrix4x4 matrix = Matrix4x4.TRS(pos, rot, scale);
			cmd.DrawMesh(mesh, matrix, DrawMaterials.unlitMat, 0, 0, materialProperties);
		}

		public static void Arrow(Vector3 start, Vector3 end, float thickness, float headLength, float headAngle, Color col, float t = 1)
		{
			end = Vector3.Lerp(start, end, t); // animate end point
			if (thickness <= 0 || (start - end).sqrMagnitude == 0) { return; }
			PerDrawInit();

			Vector2 dir2D = ((Vector2)end - (Vector2)start).normalized;
			float angle = Mathf.Atan2(dir2D.y, dir2D.x);

			float arrowT = Seb.Ease.Quadratic.Out(t * 2f);

			float headAngleA = angle + Mathf.PI + headAngle * arrowT;
			float headAngleB = angle + Mathf.PI - headAngle * arrowT;
			Vector3 headDirA = new Vector2(Mathf.Cos(headAngleA), Mathf.Sin(headAngleA));
			Vector3 headDirB = new Vector2(Mathf.Cos(headAngleB), Mathf.Sin(headAngleB));

			Line(start, end, thickness, col);
			Line(end, end + headDirA * headLength, thickness, col, arrowT);
			Line(end, end + headDirB * headLength, thickness, col, arrowT);
		}

		public static void Line(Vector3 start, Vector3 end, float thickness, Color col, float t = 1)
		{
			end = Vector3.Lerp(start, end, t); // animate end point
			if (thickness <= 0 || (start - end).sqrMagnitude == 0) { return; }

			PerDrawInit();
			float length = (start - end).magnitude;
			// Squish the rounding effect to 0 as line length goes from thickness -> 0
			float thicknessScaleT = Mathf.Min(1, length / thickness);
			Vector3 scale = new Vector3(length + thickness * thicknessScaleT, thickness, 1);

			materialProperties.SetColor(DrawMaterials.colorID, col);
			materialProperties.SetVector(DrawMaterials.sizeID, new Vector3(length + thickness, thickness, 1));

			Vector3 centre = (start + end) / 2;
			Quaternion rot = Quaternion.FromToRotation(Vector3.left, start - end);

			Matrix4x4 matrix = Matrix4x4.TRS(centre, rot, scale);
			cmd.DrawMesh(QuadMeshGenerator.GetMesh(), matrix, DrawMaterials.lineMat, 0, 0, materialProperties);
		}

		public static void Ray(Vector3 start, Vector3 offset, float thickness, Color col)
		{
			Line(start, start + offset, thickness, col);
		}

		public static void Path(Vector2[] points, float thickness, bool closed, Color col)
		{
			Vector3[] points3D = new Vector3[points.Length];
			for (int i = 0; i < points.Length; i++)
			{
				points3D[i] = new Vector3(points[i].x, points[i].y, 0);
			}
			Path(points3D, thickness, closed, col);
		}

		public static void Path(Vector3[] points, float thickness, bool closed, Color col)
		{
			// TODO: instancing stuff
			int num = closed ? points.Length : points.Length - 1;
			for (int i = 0; i < num; i++)
			{
				int next = i + 1 >= points.Length ? 0 : i + 1;
				Line(points[i], points[next], thickness, col);
			}
		}

		public static void Point(Vector3 centre, float radius, Color col)
		{
			PerDrawInit();
			materialProperties.SetColor(DrawMaterials.colorID, col);
			Matrix4x4 matrix = Matrix4x4.TRS(centre, Quaternion.identity, new Vector3(radius, radius, 1));
			cmd.DrawMesh(QuadMeshGenerator.GetMesh(), matrix, DrawMaterials.pointMat, 0, 0, materialProperties);
		}

		public static void Sphere(Vector3 centre, float radius, Color col, bool unlit = false)
		{
			PerDrawInit();
			materialProperties.SetColor(DrawMaterials.colorID, col);
			Matrix4x4 matrix = Matrix4x4.TRS(centre, Quaternion.identity, Vector3.one * radius);
			cmd.DrawMesh(SphereMeshGenerator.GetMesh(), matrix, unlit ? DrawMaterials.unlitMat : DrawMaterials.shadedMat, 0, 0, materialProperties);
		}

		public static void Cube(Vector3 centre, Quaternion rotation, Vector3 scale, Color col)
		{
			PerDrawInit();
			materialProperties.SetColor(DrawMaterials.colorID, col);
			Matrix4x4 matrix = Matrix4x4.TRS(centre, rotation, scale);
			cmd.DrawMesh(CubeMeshGenerator.GetMesh(), matrix, DrawMaterials.unlitMat, 0, 0, materialProperties);
		}

		static void OnPreRender(Camera cam)
		{
			// Don't render visualizations in reflection probe cam, or if culling mask is set to none
			if (cam.cameraType == CameraType.Reflection || cam.cullingMask == 0)
			{
				return;
			}

			CameraEvent drawCameraEvent = CameraEvent.BeforeImageEffects;

			var allBuffers = cam.GetCommandBuffers(drawCameraEvent);

			// Remove buffer by name.
			// This is done because there are situations in which the buffer can be
			// null (but still attached to camera), and I don't want to think about it.
			foreach (var b in allBuffers)
			{
				if (string.Equals(b.name, buffName, System.StringComparison.Ordinal))
				{
					cam.RemoveCommandBuffer(drawCameraEvent, b);
				}
			}

			if (lastFrameWithDrawCommands == Time.frameCount && cmd != null)
			{
				cam.AddCommandBuffer(drawCameraEvent, cmd);
			}

		}

		// Called every frame by every draw function
		static void PerDrawInit()
		{
			// Only need to init if this is the first draw request of current frame
			if (lastFrameWithDrawCommands != Time.frameCount)
			{
				lastFrameWithDrawCommands = Time.frameCount;
				if (cmd == null)
				{
					cmd = new CommandBuffer();
					cmd.name = buffName;
				}
				cmd.Clear();

				DrawMaterials.Init();
				if (materialProperties == null)
				{
					materialProperties = new MaterialPropertyBlock();
				}

				meshPool ??= new MeshPool();
				meshPool.FinishedUsingAllMeshes();
			}

		}

		// Called on enter playmode (before awake), and on script recompile in editor
		static void Init()
		{
			lastFrameWithDrawCommands = -1;
			Camera.onPreRender -= OnPreRender;
			Camera.onPreRender += OnPreRender;
		}

#if UNITY_EDITOR
		[UnityEditor.Callbacks.DidReloadScripts]
#endif
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void InitializeOnLoad()
		{
			Init();
		}


	}
}