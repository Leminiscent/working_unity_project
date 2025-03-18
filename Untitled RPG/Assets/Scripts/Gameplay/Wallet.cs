using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Wallet : MonoBehaviour, ISavable
{
    [field: SerializeField, FormerlySerializedAs("_money")] public float Money { get; private set; }

    public event Action OnMoneyChanged;
    public static Wallet Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void AddMoney(float amount)
    {
        Money += amount;
        OnMoneyChanged?.Invoke();
    }

    public void SpendMoney(float amount)
    {
        if (HasEnoughMoney(amount))
        {
            Money -= amount;
            OnMoneyChanged?.Invoke();
        }
        else
        {
            Debug.LogWarning("Not enough money to spend.");
        }
    }

    public bool HasEnoughMoney(float amount)
    {
        return Money >= amount;
    }

    public object CaptureState()
    {
        return Money;
    }

    public void RestoreState(object state)
    {
        Money = (float)state;
    }
}