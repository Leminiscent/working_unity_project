using System.Linq;
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
            // Cannot revive if the battler is not fainted.
            if (battler.Hp > 0)
            {
                return false;
            }

            // Apply revive: use half max HP for standard revive, full HP for max revive.
            battler.IncreaseHP(_revive ? battler.MaxHp / 2 : battler.MaxHp);
            battler.CureStatus();
            return true;
        }

        // If not a revive item, the battler must not be fainted.
        if (battler.Hp == 0)
        {
            return false;
        }

        if (_restoreMaxHP || _hpAmount > 0)
        {
            // Cannot heal if HP is already full.
            if (battler.Hp == battler.MaxHp)
            {
                return false;
            }

            battler.IncreaseHP(_restoreMaxHP ? battler.MaxHp : _hpAmount);
        }

        if (_recoverAllStatus || _status != ConditionID.None)
        {
            // Cannot cure status if the battler has no statuses.
            bool hasStatuses = (battler.Statuses != null && battler.Statuses.Count > 0) ||
                               (battler.VolatileStatuses != null && battler.VolatileStatuses.Count > 0);
            if (!hasStatuses)
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

        // Cannot restore SP if all moves are already full.
        if (battler.Moves.All(m => m.Sp == m.Base.SP))
        {
            return false;
        }

        // If _restoreMaxSP is true, restore to max SP; otherwise, restore by _spAmount.
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