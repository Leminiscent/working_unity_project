using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(BattlerBase))]
public class BattlerBaseEditor : Editor
{
    // Foldout flags for organizing derived attributes
    private bool showDerivedAttributes = true;
    private bool showStats = true;
    private bool showExpGpRecruitment = true;
    private bool showPvYields = true;

    public override void OnInspectorGUI()
    {
        // Draw the default inspector fields first
        base.OnInspectorGUI();

        BattlerBase battlerBase = (BattlerBase)target;

        EditorGUILayout.Space();
        showDerivedAttributes = EditorGUILayout.Foldout(showDerivedAttributes, "Derived Attributes");
        if (showDerivedAttributes)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.Space();

            // Draw Stats Section
            showStats = EditorGUILayout.Foldout(showStats, "Stats");
            if (showStats)
            {
                DrawStats(battlerBase);
            }

            EditorGUILayout.Space();

            // Draw Experience, GP, and Recruitment Section
            showExpGpRecruitment = EditorGUILayout.Foldout(showExpGpRecruitment, "Exp, GP, and Recruitment");
            if (showExpGpRecruitment)
            {
                DrawExpGpRecruitment(battlerBase);
            }

            EditorGUILayout.Space();

            // Draw PV Yields Section
            showPvYields = EditorGUILayout.Foldout(showPvYields, "PV Yields");
            if (showPvYields)
            {
                DrawPvYields(battlerBase);
            }

            EditorGUILayout.EndVertical();
        }
    }

    private void DrawStats(BattlerBase battlerBase)
    {
        EditorGUILayout.LabelField("HP", battlerBase.HP.ToString());
        EditorGUILayout.LabelField("Strength", battlerBase.Strength.ToString());
        EditorGUILayout.LabelField("Endurance", battlerBase.Endurance.ToString());
        EditorGUILayout.LabelField("Intelligence", battlerBase.Intelligence.ToString());
        EditorGUILayout.LabelField("Fortitude", battlerBase.Fortitude.ToString());
        EditorGUILayout.LabelField("Agility", battlerBase.Agility.ToString());
        EditorGUILayout.LabelField("Total Stats", battlerBase.TotalStats.ToString());
    }

    private void DrawExpGpRecruitment(BattlerBase battlerBase)
    {
        EditorGUILayout.LabelField("Base Exp Yield", battlerBase.ExpYield.ToString());
        EditorGUILayout.LabelField("Growth Rate", battlerBase.GrowthRate.ToString());
        EditorGUILayout.LabelField("Base GP Yield", $"{battlerBase.BaseGp.x} - {battlerBase.BaseGp.y + 1}");
        EditorGUILayout.LabelField("Recruit Rate", battlerBase.RecruitRate.ToString());
    }

    private void DrawPvYields(BattlerBase battlerBase)
    {
        foreach (KeyValuePair<Stat, float> yield in battlerBase.PvYield)
        {
            EditorGUILayout.LabelField(yield.Key.ToString(), yield.Value.ToString("F2"));
        }
    }
}