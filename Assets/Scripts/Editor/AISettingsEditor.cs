namespace Chess.EditorScripts {

	using System.Collections.Generic;
	using System.Collections;
	using UnityEditor;
	using UnityEngine;

	[CustomEditor (typeof (AISettings))]
	public class AISettingsEditor : Editor {

		public override void OnInspectorGUI () {
			DrawDefaultInspector ();

			AISettings settings = target as AISettings;

			if (settings.useThreading) {
				if (GUILayout.Button ("Abort Search")) {
					settings.RequestAbortSearch ();
				}
			}
		}
	}

}