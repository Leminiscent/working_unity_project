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

        // For non-revive items, ensure the battler is not fainted.
        if (battler.Hp == 0)
        {
            return false;
        }

        bool effectApplied = false;

        // Attempt HP restoration if applicable.
        if (_restoreMaxHP || _hpAmount > 0)
        {
            if (battler.Hp < battler.MaxHp)
            {
                battler.IncreaseHP(_restoreMaxHP ? battler.MaxHp : _hpAmount);
                effectApplied = true;
            }
        }

        // Attempt status condition recovery if applicable.
        if (_recoverAllStatus || _status != ConditionID.None)
        {
            bool hasStatuses = (battler.Statuses != null && battler.Statuses.Count > 0) ||
                               (battler.VolatileStatuses != null && battler.VolatileStatuses.Count > 0);
            if (hasStatuses)
            {
                if (_recoverAllStatus)
                {
                    battler.CureAllStatus();
                    effectApplied = true;
                }
                else
                {
                    if (battler.Statuses != null && battler.Statuses.ContainsKey(_status))
                    {
                        battler.CureStatus();
                        effectApplied = true;
                    }
                    else if (battler.VolatileStatuses != null && battler.VolatileStatuses.ContainsKey(_status))
                    {
                        battler.CureVolatileStatus();
                        effectApplied = true;
                    }
                }
            }
        }

        // Attempt SP restoration if applicable.
        if (_restoreMaxSP || _spAmount > 0)
        {
            bool spRestored = false;
            foreach (Move move in battler.Moves)
            {
                if (move.Sp < move.Base.SP)
                {
                    if (_restoreMaxSP)
                    {
                        move.RestoreSP(move.Base.SP);
                    }
                    else
                    {
                        move.RestoreSP(_spAmount);
                    }
                    spRestored = true;
                }
            }
            if (spRestored)
            {
                effectApplied = true;
            }
        }

        return effectApplied;
    }
}