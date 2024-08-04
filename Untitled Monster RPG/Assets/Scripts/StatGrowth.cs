using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatGrowth
{
    public int CalculateHP(int baseHP, float hpGrowthRate, int level)
    {
        return baseHP + (int)(hpGrowthRate * Mathf.Pow(level - 1, 2));
    }

    public int CalculateMP(int baseMP, float mpGrowthRate, int level)
    {
        return baseMP + (int)(mpGrowthRate * Mathf.Log(level));
    }

    public int CalculateStrength(int baseStrength, float strengthGrowthRate, int level)
    {
        return baseStrength + (int)(strengthGrowthRate * (level - 1));
    }

    public int CalculateAgility(int baseAgility, float agilityGrowthRate, int level)
    {
        return baseAgility + (int)(agilityGrowthRate * (level - 1));
    }

    public int CalculateIntelligence(int baseIntelligence, float intelligenceGrowthRate, int level)
    {
        return baseIntelligence * (int)Mathf.Pow(1 + intelligenceGrowthRate, level - 1);
    }

    public int CalculateStamina(int baseStamina, float staminaGrowthRate, int level)
    {
        return baseStamina + (int)(staminaGrowthRate * (level - 1));
    }

    public int CalculateLuck(int baseLuck, float luckGrowthRate, int level)
    {
        return baseLuck + (int)(luckGrowthRate * Mathf.Log(level));
    }
}