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
        int totalChanceOnGround = serializedObject.FindProperty("totalChance").intValue;
        int totalChanceInWater = serializedObject.FindProperty("totalChanceWater").intValue;

        if (totalChanceOnGround != 100 && totalChanceOnGround != -1)
        {
            EditorGUILayout.HelpBox($"The total ground spawn chance is {totalChanceOnGround}%. It should be 100%", MessageType.Error);
        }
        if (totalChanceInWater != 100 && totalChanceInWater != -1)
        {
            EditorGUILayout.HelpBox($"The total water spawn chance is {totalChanceInWater}%. It should be 100%", MessageType.Error);
        }
    }
}
