using TMPro;
using UnityEngine;

public class WalletUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _moneyText;

    private void Start()
    {
        Wallet.Instance.OnMoneyChanged += SetMoneyText;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        SetMoneyText();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void SetMoneyText()
    {
        _moneyText.text = $"{Wallet.Instance.Money} GP";
    }
}
