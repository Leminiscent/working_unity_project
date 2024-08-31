using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new scroll")]
public class Scroll : ItemBase
{
    public override bool Use(Monster monster)
    {
        if (GameController.Instance.State == GameState.Battle)
        {
            return true;
        }
        return false;
    }
}
