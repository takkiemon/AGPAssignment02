using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CubeMeshScript))]
public class CubeMeshScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CubeMeshScript meshObject = (CubeMeshScript)target;
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
