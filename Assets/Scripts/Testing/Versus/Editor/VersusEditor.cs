using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Chess.Testing.Versus
{
	[CustomEditor(typeof(VersusManager))]
	public class VersusEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			
			if (GUILayout.Button("Open Games Folder"))
			{
				System.IO.Directory.CreateDirectory(VersusManager.GamesSaveFolder);
				EditorUtility.RevealInFinder(VersusManager.GamesSaveFolder);
			}
		}
	}
}