using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapArea))]
public class MapAreaEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        int totalChance = serializedObject.FindProperty("totalChance").intValue;
        var style = new GUIStyle(GUI.skin.label);

        style.fontStyle = FontStyle.Bold;
        GUILayout.Label($"Total spawn chance: {totalChance}%", style);
        if (totalChance != 100)
        {
            EditorGUILayout.HelpBox("The total spawn chance is not 100%", MessageType.Error);
        }
    }
}
