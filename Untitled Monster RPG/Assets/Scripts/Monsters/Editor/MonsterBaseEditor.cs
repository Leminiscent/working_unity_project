using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(MonsterBase))]
public class MonsterBaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MonsterBase monsterBase = (MonsterBase)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Derived Stats", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Growth Rate", monsterBase.GrowthRate.ToString());
        EditorGUILayout.LabelField("Recruit Rate", monsterBase.RecruitRate.ToString());

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Pv Yields", EditorStyles.boldLabel);

        foreach (KeyValuePair<Stat, int> yield in monsterBase.PvYield)
        {
            EditorGUILayout.LabelField($"{yield.Key}", yield.Value.ToString());
        }
    }
}