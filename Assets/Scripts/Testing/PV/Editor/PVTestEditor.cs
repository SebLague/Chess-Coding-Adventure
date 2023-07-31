using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Chess.Testing.EditorScripts
{
	[CustomEditor(typeof(PVTest))]
	public class PVTestEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			using (new EditorGUI.DisabledScope(!Application.isPlaying))
			{
				PVTest test = target as PVTest;

				if (GUILayout.Button("Evaluate (Static)"))
				{
					test.RunStaticEvaluation();
				}
				if (GUILayout.Button("Evaluate (Qsearch)"))
				{
					test.RunQSearchEvaluation();
				}
			}
		}
	}
}