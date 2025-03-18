using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleParty : MonoBehaviour
{
    [SerializeField] private List<Battler> _battlers;

    private BattlerStorage _storage;

    public event Action OnUpdated;
    public List<Battler> Battlers
    {
        get => _battlers;
        set
        {
            _battlers = value;
            OnUpdated?.Invoke();
        }
    }

    private void Awake()
    {
        _storage = GetComponent<BattlerStorage>();
        foreach (Battler battler in _battlers)
        {
            battler.InitNewBattler();
        }
    }

    public Battler GetHealthyBattlers(List<Battler> excludedBattlers = null)
    {
        List<Battler> healthyBattlers = _battlers.Where(static x => x.Hp > 0).ToList();

        if (excludedBattlers != null)
        {
            healthyBattlers = healthyBattlers.Where(x => !excludedBattlers.Contains(x)).ToList();
        }

        return healthyBattlers.FirstOrDefault();
    }

    public List<Battler> GetHealthyBattlers(int count)
    {
        return _battlers.Where(static x => x.Hp > 0).Take(count).ToList();
    }

    public void AddMember(Battler newBattler)
    {
        if (_battlers.Count < 6)
        {
            _battlers.Add(newBattler);
            OnUpdated?.Invoke();
        }
        else
        {
            _storage.AddBattlerToFirstEmptySlot(newBattler);
        }
    }

    public bool CheckForTransformations()
    {
        return _battlers.Any(static b => b.HasJustLeveledUp && b.CheckForTransformation() != null);
    }


    public IEnumerator RunTransformations()
    {
        foreach (Battler battler in _battlers)
        {
            if (battler.HasJustLeveledUp)
            {
                Transformation transformation = battler.CheckForTransformation();
                if (transformation != null)
                {
                    yield return TransformationState.Instance.Transform(battler, transformation);
                }
            }
        }
    }

    public void RestoreParty()
    {
        foreach (Battler battler in _battlers)
        {
            battler.RestoreBattler();
        }
    }

    public void PartyUpdated()
    {
        OnUpdated?.Invoke();
    }

    public static BattleParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<BattleParty>();
    }
}
