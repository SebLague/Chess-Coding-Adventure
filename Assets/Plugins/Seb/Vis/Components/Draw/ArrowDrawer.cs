using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Seb.Vis.Internal;

namespace Seb.Vis
{
	[ExecuteAlways]
	public class ArrowDrawer : DrawerBase
	{
		[Header("Draw Settings")]
		public Vector3 offset = Vector3.up;
		public Color col = Color.white;
		public float thickness = 0.06f;
		public float headLength = 0.2f;
		public float headAngle = 30;

		[Header("Animate Settings")]
		[Range(0, 1)] public float t = 1;
		public Ease.EaseType easeFunction;
		public Ease.EaseDirection easeDirection;

		void Update()
		{
			
			Vector3 startPoint = transform.position;
			Vector3 endPoint = startPoint + offset;
			float easedT = Ease.GetEasing(t, easeFunction, easeDirection);
			Draw.Arrow(startPoint, endPoint, thickness, headLength, headAngle * Mathf.Deg2Rad, col, easedT);
		}

		public override void SetTime(float t) => this.t = t;

	}
}