using UnityEditor;

[CustomEditor(typeof(MapArea))]
public class MapAreaEditor : Editor
{
    private SerializedProperty totalChanceProp;

    private void OnEnable()
    {
        totalChanceProp = serializedObject.FindProperty("_totalChance");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        int totalChance = totalChanceProp.intValue;

        if (totalChance is not 100 and not (-1))
        {
            EditorGUILayout.HelpBox($"The total spawn chance is {totalChance}%. It should be 100%", MessageType.Error);
        }
    }
}