using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SaveController))]
public class SaveControllerEditor : Editor {

	public override void OnInspectorGUI ()
	{
		 SaveController loader = target as SaveController;

		 if (GUILayout.Button ("Clear Save Data")) {
			loader.clearSettings();
		 }

		 DrawDefaultInspector ();
	}
}
