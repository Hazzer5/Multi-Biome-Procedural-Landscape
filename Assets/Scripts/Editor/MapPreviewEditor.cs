using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapPreview))]
public class MapPreviewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapPreview mapPreview = (MapPreview)target;

        DrawDefaultInspector();

        if (GUILayout.Button ("Generate")) {
            mapPreview.DrawInEditor();
        }
    }
}
