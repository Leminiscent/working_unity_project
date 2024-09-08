using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new transformation item")]
public class TransformationItem : ItemBase
{
    public override bool Use(Monster monster)
    {
        return true;
    }
}