using System;
using UnityEngine;

public class Wallet : MonoBehaviour, ISavable
{
    [SerializeField] float money;

    public event Action OnMoneyChanged;

    public static Wallet Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void AddMoney(float amount)
    {
        money += amount;
        OnMoneyChanged?.Invoke();
    }

    public void SpendMoney(float amount)
    {
        money -= amount;
        OnMoneyChanged?.Invoke();
    }

    public bool HasEnoughMoney(float amount)
    {
        return money >= amount;
    }

    public Wallet GetWallet()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Wallet>();
    }

    public object CaptureState()
    {
        return money;
    }

    public void RestoreState(object state)
    {
        money = (float)state;
    }

    public float Money => money;
}
