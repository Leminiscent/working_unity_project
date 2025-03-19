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

    // Trigger update event on set.
    public List<Battler> Battlers
    {
        get => _battlers;
        set
        {
            _battlers = value;
            OnUpdated?.Invoke();
        }
    }

    // Constant for maximum party members.
    private const int MAX_PARTY_MEMBERS = 6;

    private void Awake()
    {
        _storage = GetComponent<BattlerStorage>();

        foreach (Battler battler in _battlers)
        {
            battler.InitBattler();
        }
    }

    public Battler GetHealthyBattlers(List<Battler> excludedBattlers = null)
    {
        List<Battler> healthyBattlers = _battlers.Where(b => b.Hp > 0).ToList();

        if (excludedBattlers != null)
        {
            healthyBattlers = healthyBattlers.Where(b => !excludedBattlers.Contains(b)).ToList();
        }

        return healthyBattlers.FirstOrDefault();
    }

    public List<Battler> GetHealthyBattlers(int count)
    {
        return _battlers.Where(static b => b.Hp > 0).Take(count).ToList();
    }

    public void AddMember(Battler newBattler)
    {
        if (_battlers.Count < MAX_PARTY_MEMBERS)
        {
            _battlers.Add(newBattler);
            OnUpdated?.Invoke();
        }
        else
        {
            _storage.AddBattlerToFirstEmptySlot(newBattler);
        }
    }

    public void PartyUpdated()
    {
        OnUpdated?.Invoke();
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
                    // Wait for the transformation process to complete.
                    yield return TransformationState.Instance.PerformTransformation(battler, transformation);
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

    public static BattleParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<BattleParty>();
    }
}