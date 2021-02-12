namespace Chess.EditorScripts {
	using System.Collections.Generic;
	using System.Collections;
	using UnityEditor;
	using UnityEngine;

	[CustomEditor (typeof (Chess.Game.GameManager))]
	public class GameManagerEditor : Editor {

		Editor aiSettingsEditor;

		public override void OnInspectorGUI () {
			base.OnInspectorGUI ();
			var manager = target as Chess.Game.GameManager;

			bool foldout = true;
			DrawSettingsEditor (manager.aiSettings, ref foldout, ref aiSettingsEditor);
		}

		void DrawSettingsEditor (Object settings, ref bool foldout, ref Editor editor) {
			if (settings != null) {
				foldout = EditorGUILayout.InspectorTitlebar (foldout, settings);
				if (foldout) {
					CreateCachedEditor (settings, null, ref editor);
					editor.OnInspectorGUI ();
				}
			}
		}

	}
}