using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Merchant : MonoBehaviour
{
    [SerializeField] private List<ItemBase> _itemsForSale;

    public List<ItemBase> ItemsForSale => _itemsForSale;

    public IEnumerator Trade()
    {
        ShopMenuState.Instance.AvailableItems = _itemsForSale;
        yield return GameController.Instance.StateMachine.PushAndWait(ShopMenuState.Instance);
    }
}
