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

    public override bool Use(Battler battler)
    {
        if (_revive || _maxRevive)
        {
            if (battler.Hp > 0)
            {
                return false;
            }

            if (_revive)
            {
                battler.IncreaseHP(battler.MaxHp / 2);
            }
            else
            {
                battler.IncreaseHP(battler.MaxHp);
            }

            battler.CureStatus();
            return true;
        }

        if (battler.Hp == 0)
        {
            return false;
        }

        if (_restoreMaxHP || _hpAmount > 0)
        {
            if (battler.Hp == battler.MaxHp)
            {
                return false;
            }

            if (_restoreMaxHP)
            {
                battler.IncreaseHP(battler.MaxHp);
            }
            else
            {
                battler.IncreaseHP(_hpAmount);
            }
        }

        if (_recoverAllStatus || _status != ConditionID.None)
        {
            if ((battler.Statuses == null && battler.VolatileStatuses == null) || (battler.Statuses.Count == 0 && battler.VolatileStatuses.Count == 0))
            {
                return false;
            }

            if (_recoverAllStatus)
            {
                battler.CureAllStatus();
            }
            else
            {
                if (battler.Statuses != null && battler.Statuses.ContainsKey(_status))
                {
                    battler.CureStatus();
                }
                else if (battler.VolatileStatuses != null && battler.VolatileStatuses.ContainsKey(_status))
                {
                    battler.CureVolatileStatus();
                }
                else
                {
                    return false;
                }
            }
        }

        if (_restoreMaxSP)
        {
            battler.Moves.ForEach(m => m.RestoreSP(m.Base.SP));
        }
        else if (_spAmount > 0)
        {
            battler.Moves.ForEach(m => m.RestoreSP(_spAmount));
        }

        return true;
    }
}
