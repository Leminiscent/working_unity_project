using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Merchant : MonoBehaviour
{
    [field: SerializeField, FormerlySerializedAs("_itemsForSale")] public List<ItemBase> ItemsForSale { get; private set; }

    public IEnumerator Trade()
    {
        ShopMenuState.Instance.AvailableItems = ItemsForSale;
        yield return GameController.Instance.StateMachine.PushAndWait(ShopMenuState.Instance);
    }
}
