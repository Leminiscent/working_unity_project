using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new recovery item")]
public class RecoveryItem : ItemBase
{
    [Header("HP")]
    [SerializeField] private int _hpAmount;
    [SerializeField] private bool _restoreMaxHP;

    [Header("SP")]
    [SerializeField] private int _spAmount;
    [SerializeField] private bool _restoreMaxSP;

    [Header("Status Conditions")]
    [SerializeField] private ConditionID _status;
    [SerializeField] private bool _recoverAllStatus;

    [Header("Revive")]
    [SerializeField] private bool _revive;
    [SerializeField] private bool _maxRevive;

    public override bool Use(Monster monster)
    {
        if (_revive || _maxRevive)
        {
            if (monster.Hp > 0)
            {
                return false;
            }

            if (_revive)
            {
                monster.IncreaseHP(monster.MaxHp / 2);
            }
            else
            {
                monster.IncreaseHP(monster.MaxHp);
            }

            monster.CureStatus();
            return true;
        }

        if (monster.Hp == 0)
        {
            return false;
        }

        if (_restoreMaxHP || _hpAmount > 0)
        {
            if (monster.Hp == monster.MaxHp)
            {
                return false;
            }

            if (_restoreMaxHP)
            {
                monster.IncreaseHP(monster.MaxHp);
            }
            else
            {
                monster.IncreaseHP(_hpAmount);
            }
        }

        if (_recoverAllStatus || _status != ConditionID.None)
        {
            if ((monster.Statuses == null && monster.VolatileStatuses == null) || (monster.Statuses.Count == 0 && monster.VolatileStatuses.Count == 0))
            {
                return false;
            }

            if (_recoverAllStatus)
            {
                monster.CureAllStatus();
            }
            else
            {
                if (monster.Statuses != null && monster.Statuses.ContainsKey(_status))
                {
                    monster.CureStatus();
                }
                else if (monster.VolatileStatuses != null && monster.VolatileStatuses.ContainsKey(_status))
                {
                    monster.CureVolatileStatus();
                }
                else
                {
                    return false;
                }
            }
        }

        if (_restoreMaxSP)
        {
            monster.Moves.ForEach(m => m.RestoreSP(m.Base.SP));
        }
        else if (_spAmount > 0)
        {
            monster.Moves.ForEach(m => m.RestoreSP(_spAmount));
        }

        return true;
    }
}
