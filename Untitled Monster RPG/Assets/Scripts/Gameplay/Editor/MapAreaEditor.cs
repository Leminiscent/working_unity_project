using UnityEditor;

[CustomEditor(typeof(MapArea))]
public class MapAreaEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        int totalChance = serializedObject.FindProperty("totalChance").intValue;

        if (totalChance != 100 && totalChance != -1)
        {
            EditorGUILayout.HelpBox($"The total spawn chance is {totalChance}%. It should be 100%", MessageType.Error);
        }
    }
}
