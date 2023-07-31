using UnityEngine;

namespace Seb.Vis.Internal
{
	public static class DrawMaterials
	{

		public static Material unlitMat { get; private set; }
		public static Material shadedMat { get; private set; }
		public static Material pointMat { get; private set; }
		public static Material lineMat { get; private set; }
		public static Material quadMat { get; private set; }

		public static readonly int colorID = Shader.PropertyToID("_Color");
		public static readonly int sizeID = Shader.PropertyToID("_Size");

		public static readonly int quadPointA = Shader.PropertyToID("PosA");
		public static readonly int quadPointB = Shader.PropertyToID("PosB");
		public static readonly int quadPointC = Shader.PropertyToID("PosC");
		public static readonly int quadPointD = Shader.PropertyToID("PosD");

		public static void Init()
		{

			if (unlitMat == null)
			{
				unlitMat = new Material(Shader.Find("Vis/UnlitColorAlpha"));
			}
			if (shadedMat == null)
			{
				shadedMat = new Material(Shader.Find("Vis/Shaded"));
			}
			if (pointMat == null)
			{
				pointMat = new Material(Shader.Find("Vis/UnlitPoint"));
			}
			if (lineMat == null)
			{
				lineMat = new Material(Shader.Find("Vis/Line"));
			}
			if (quadMat == null)
			{
				quadMat = new Material(Shader.Find("Vis/Quad"));
			}
		}
	}
}