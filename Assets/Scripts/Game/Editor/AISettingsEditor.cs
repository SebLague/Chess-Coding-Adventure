using System.Collections.Generic;
using System.Collections;
using UnityEditor;
using UnityEngine;
using Chess.Core;

namespace Chess.Game.EditorScripts
{

	[CustomEditor(typeof(AISettings))]
	public class AISettingsEditor : Editor
	{

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			AISettings settings = target as AISettings;

			if (settings.runOnMainThread)
			{
				if (GUILayout.Button("Cancel Search"))
				{
					settings.RequestCancelSearch();
				}
			}
		}
	}

}