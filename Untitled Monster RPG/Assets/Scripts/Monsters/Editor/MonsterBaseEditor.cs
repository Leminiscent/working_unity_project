using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(MonsterBase))]
public class MonsterBaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MonsterBase monsterBase = (MonsterBase)target;

        float sumOfWeights = serializedObject.FindProperty("_sumOfWeights").floatValue;

        if (sumOfWeights != 1)
        {
            EditorGUILayout.HelpBox($"The sum of all individual stat weights is {sumOfWeights}. It should be equal to 1", MessageType.Error);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Derived Stats", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("HP", monsterBase.HP.ToString());
        EditorGUILayout.LabelField("Strength", monsterBase.Strength.ToString());
        EditorGUILayout.LabelField("Endurance", monsterBase.Endurance.ToString());
        EditorGUILayout.LabelField("Intelligence", monsterBase.Intelligence.ToString());
        EditorGUILayout.LabelField("Fortitude", monsterBase.Fortitude.ToString());
        EditorGUILayout.LabelField("Agility", monsterBase.Agility.ToString());
        EditorGUILayout.LabelField("Total Stats", monsterBase.TotalStats.ToString());
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Derived Experience and Recruitment Attributes", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Exp Yield", monsterBase.ExpYield.ToString());
        EditorGUILayout.LabelField("Growth Rate", monsterBase.GrowthRate.ToString());
        EditorGUILayout.LabelField("Recruit Rate", monsterBase.RecruitRate.ToString());
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Derived PV Yields", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");
        foreach (KeyValuePair<Stat, float> yield in monsterBase.PvYield)
        {
            EditorGUILayout.LabelField($"{yield.Key}", yield.Value.ToString("F2"));
        }
        EditorGUILayout.EndVertical();
    }
}