using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new recovery item")]
public class RecoveryItem : ItemBase
{
    [Header("HP")]
    [SerializeField] int hpAmount;
    [SerializeField] bool restoreMaxHP;

    [Header("AP")]
    [SerializeField] int apAmount;
    [SerializeField] bool restoreMaxAP;

    [Header("Status Conditions")]
    [SerializeField] ConditionID status;
    [SerializeField] bool recoverAllStatus;

    [Header("Revive")]
    [SerializeField] bool revive;
    [SerializeField] bool maxRevive;

    public override bool Use(Monster monster)
    {
        if (hpAmount > 0)
        {
            if (monster.HP == monster.MaxHp)
            {
                return false;
            }
            monster.IncreaseHP(hpAmount);
        }

        return true;
    }
}
