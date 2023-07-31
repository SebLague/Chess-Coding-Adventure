using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Seb.Vis.Internal;
using UnityEngine.InputSystem;

namespace Seb.Vis
{
	public class DrawAnimator : MonoBehaviour
	{
		public DrawerBase drawer;
		public float speed = 1;
		public float delay = 0.1f;

		float t;

		void Start()
		{
			drawer.SetTime(0);
		}

		void Update()
		{

			if (Time.time > delay)
			{
				t += Time.deltaTime * speed;
			}

			drawer.SetTime(t);
		}

		public bool Finished => t >= 1;

		void OnValidate()
		{
			if (drawer == null)
			{
				drawer = GetComponent<DrawerBase>();
			}
		}
	}
}