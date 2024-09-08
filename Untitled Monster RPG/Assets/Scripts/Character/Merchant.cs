using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Merchant : MonoBehaviour
{
    [SerializeField] List<ItemBase> itemsForSale;
    
    public IEnumerator Trade()
    {
        yield return ShopController.Instance.StartTrading(this);
    }

    public List<ItemBase> ItemsForSale => itemsForSale;
}
