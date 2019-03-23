using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GenerateFaces))]
public class GenerateFacesEditor : Editor {

	public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GenerateFaces meshObject = (GenerateFaces)target;
        if (GUILayout.Button("rebuildMesh"))
        {
            meshObject.showMesh = true;
            meshObject.RebuildMesh();
        }
        if (GUILayout.Button("clearMesh"))
        {
            meshObject.showMesh = false;
            meshObject.ClearMesh();
        }
    }
}