using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new recovery item")]
public class RecoveryItem : ItemBase
{
    [Header("HP")]
    [SerializeField] int hpAmount;
    [SerializeField] bool restoreMaxHP;

    [Header("SP")]
    [SerializeField] int spAmount;
    [SerializeField] bool restoreMaxSP;

    [Header("Status Conditions")]
    [SerializeField] ConditionID status;
    [SerializeField] bool recoverAllStatus;

    [Header("Revive")]
    [SerializeField] bool revive;
    [SerializeField] bool maxRevive;

    public override bool Use(Monster monster)
    {
        if (revive || maxRevive)
        {
            if (monster.HP > 0)
            {
                return false;
            }

            if (revive)
            {
                monster.IncreaseHP(monster.MaxHP / 2);
            }
            else
            {
                monster.IncreaseHP(monster.MaxHP);
            }

            monster.CureStatus();
            return true;
        }

        if (monster.HP == 0)
        {
            return false;
        }

        if (restoreMaxHP || hpAmount > 0)
        {
            if (monster.HP == monster.MaxHP)
            {
                return false;
            }

            if (restoreMaxHP)
            {
                monster.IncreaseHP(monster.MaxHP);
            }
            else
            {
                monster.IncreaseHP(hpAmount);
            }
        }

        if (recoverAllStatus || status != ConditionID.none)
        {
            if (monster.Status == null && monster.VolatileStatus == null)
            {
                return false;
            }

            if (recoverAllStatus)
            {
                monster.CureStatus();
                monster.CureVolatileStatus();
            }
            else
            {
                if (monster.Status.ID == status)
                {
                    monster.CureStatus();
                }
                else if (monster.VolatileStatus.ID == status)
                {
                    monster.CureVolatileStatus();
                }
                else
                {
                    return false;
                }
            }
        }

        if (restoreMaxSP)
        {
            monster.Moves.ForEach(m => m.RestoreSP(m.Base.SP));
        }
        else if (spAmount > 0)
        {
            monster.Moves.ForEach(m => m.RestoreSP(spAmount));
        }

        return true;
    }
}
