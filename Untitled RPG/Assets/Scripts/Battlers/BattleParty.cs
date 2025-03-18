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

    // Expose battlers via a property; trigger update event on set.
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

    /// <summary>
    /// Returns the first healthy battler not contained in the optional excluded list.
    /// </summary>
    public Battler GetHealthyBattlers(List<Battler> excludedBattlers = null)
    {
        List<Battler> healthyBattlers = _battlers.Where(b => b.Hp > 0).ToList();

        if (excludedBattlers != null)
        {
            healthyBattlers = healthyBattlers.Where(b => !excludedBattlers.Contains(b)).ToList();
        }

        return healthyBattlers.FirstOrDefault();
    }

    /// <summary>
    /// Returns a list of up to the specified count of healthy battlers.
    /// </summary>
    public List<Battler> GetHealthyBattlers(int count)
    {
        return _battlers.Where(static b => b.Hp > 0).Take(count).ToList();
    }

    /// <summary>
    /// Adds a new battler to the party if there is room; otherwise stores it externally.
    /// </summary>
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

    /// <summary>
    /// Invoke the updated event.
    /// </summary>
    public void PartyUpdated()
    {
        OnUpdated?.Invoke();
    }

    /// <summary>
    /// Checks if any battler in the party is eligible for transformation after leveling up.
    /// </summary>
    public bool CheckForTransformations()
    {
        return _battlers.Any(static b => b.HasJustLeveledUp && b.CheckForTransformation() != null);
    }

    /// <summary>
    /// Runs transformations on battlers that have just leveled up.
    /// </summary>
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
                    yield return TransformationState.Instance.Transform(battler, transformation);
                }
            }
        }
    }

    /// <summary>
    /// Restores the entire party by healing all battlers and resetting statuses.
    /// </summary>
    public void RestoreParty()
    {
        foreach (Battler battler in _battlers)
        {
            battler.RestoreBattler();
        }
    }

    /// <summary>
    /// Returns the player's party by locating the PlayerController in the scene.
    /// </summary>
    public static BattleParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<BattleParty>();
    }
}