using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(FireSimulator))]
public class FireSimulatorEditor : Editor
{

	public override void OnInspectorGUI()
	{
		FireSimulator fireSim = (FireSimulator)target;

		DrawDefaultInspector();

		if (GUILayout.Button("Simulate"))
		{
			fireSim.SetFire();
		}

		if (GUILayout.Button("Hide/Show clusters"))
		{
			fireSim.switchTexture();
		}
	}
}