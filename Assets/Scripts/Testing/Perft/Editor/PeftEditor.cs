using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Chess.Testing
{
	[CustomEditor(typeof(Perft))]
	public class PerftEditor : Editor
	{

		Perft perft;

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
		
		}

		void OnEnable()
		{
			perft = (Perft)target;
		}
	}
}