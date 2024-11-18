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
        EditorGUILayout.LabelField("Derived Attributes", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("HP", monsterBase.HP.ToString());
        EditorGUILayout.LabelField("Strength", monsterBase.Strength.ToString());
        EditorGUILayout.LabelField("Endurance", monsterBase.Endurance.ToString());
        EditorGUILayout.LabelField("Intelligence", monsterBase.Intelligence.ToString());
        EditorGUILayout.LabelField("Fortitude", monsterBase.Fortitude.ToString());
        EditorGUILayout.LabelField("Agility", monsterBase.Agility.ToString());
        EditorGUILayout.LabelField("Total Stats", monsterBase.TotalStats.ToString());

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Exp, GP, and Recruitment", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Base Exp Yield", monsterBase.ExpYield.ToString());
        EditorGUILayout.LabelField("Growth Rate", monsterBase.GrowthRate.ToString());
        EditorGUILayout.LabelField("Base GP Yield", $"{monsterBase.BaseGp.x} - {monsterBase.BaseGp.y + 1}");
        EditorGUILayout.LabelField("Recruit Rate", monsterBase.RecruitRate.ToString());

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("PV Yields", EditorStyles.boldLabel);

        foreach (KeyValuePair<Stat, float> yield in monsterBase.PvYield)
        {
            EditorGUILayout.LabelField($"{yield.Key}", yield.Value.ToString("F2"));
        }

        EditorGUILayout.EndVertical();
    }
}