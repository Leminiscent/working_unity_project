using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BattlerBase))]
public class BattlerBaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        BattlerBase battlerBase = (BattlerBase)target;

        float sumOfWeights = serializedObject.FindProperty("_sumOfWeights").floatValue;

        if (!Mathf.Approximately(sumOfWeights, 1f))
        {
            EditorGUILayout.HelpBox($"The sum of all individual stat weights is {sumOfWeights}. It should be equal to 1", MessageType.Error);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Derived Attributes", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("HP", battlerBase.HP.ToString());
        EditorGUILayout.LabelField("Strength", battlerBase.Strength.ToString());
        EditorGUILayout.LabelField("Endurance", battlerBase.Endurance.ToString());
        EditorGUILayout.LabelField("Intelligence", battlerBase.Intelligence.ToString());
        EditorGUILayout.LabelField("Fortitude", battlerBase.Fortitude.ToString());
        EditorGUILayout.LabelField("Agility", battlerBase.Agility.ToString());
        EditorGUILayout.LabelField("Total Stats", battlerBase.TotalStats.ToString());

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Exp, GP, and Recruitment", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Base Exp Yield", battlerBase.ExpYield.ToString());
        EditorGUILayout.LabelField("Growth Rate", battlerBase.GrowthRate.ToString());
        EditorGUILayout.LabelField("Base GP Yield", $"{battlerBase.BaseGp.x} - {battlerBase.BaseGp.y + 1}");
        EditorGUILayout.LabelField("Recruit Rate", battlerBase.RecruitRate.ToString());

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("PV Yields", EditorStyles.boldLabel);

        foreach (KeyValuePair<Stat, float> yield in battlerBase.PvYield)
        {
            EditorGUILayout.LabelField($"{yield.Key}", yield.Value.ToString("F2"));
        }

        EditorGUILayout.EndVertical();
    }
}