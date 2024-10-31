using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Merchant : MonoBehaviour
{
    [SerializeField] private List<ItemBase> itemsForSale;
    
    public IEnumerator Trade()
    {
        ShopMenuState.Instance.AvailableItems = itemsForSale;
        yield return GameController.Instance.StateMachine.PushAndWait(ShopMenuState.Instance);
    }

    public List<ItemBase> ItemsForSale => itemsForSale;
}
