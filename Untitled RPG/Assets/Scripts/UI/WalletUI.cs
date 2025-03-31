using TMPro;
using UnityEngine;

public class WalletUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _moneyText;

    private void Start()
    {
        Wallet.Instance.OnMoneyChanged += SetMoneyText;
    }

    private void OnDestroy()
    {
        Wallet.Instance.OnMoneyChanged -= SetMoneyText;
    }

    public void Show()
    {
        gameObject.SetActive(true); // TODO: Tween in the UI.
        SetMoneyText();
    }

    public void Close()
    {
        gameObject.SetActive(false); // TODO: Tween out the UI.
    }

    private void SetMoneyText()
    {
        _moneyText.text = $"{Wallet.Instance.Money} GP";
    }
}
