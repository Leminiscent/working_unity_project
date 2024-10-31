using TMPro;
using UnityEngine;

public class WalletUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI moneyText;

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

    void SetMoneyText()
    {
        moneyText.text = $"{Wallet.Instance.Money} GP";
    }
}
