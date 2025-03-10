using System;
using UnityEngine;

public class Wallet : MonoBehaviour, ISavable
{
    [SerializeField] private float _money;

    public float Money => _money;
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
        _money += amount;
        OnMoneyChanged?.Invoke();
    }

    public void SpendMoney(float amount)
    {
        _money -= amount;
        OnMoneyChanged?.Invoke();
    }

    public bool HasEnoughMoney(float amount)
    {
        return _money >= amount;
    }

    public Wallet GetWallet()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Wallet>();
    }

    public object CaptureState()
    {
        return _money;
    }

    public void RestoreState(object state)
    {
        _money = (float)state;
    }
}
