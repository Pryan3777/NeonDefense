using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemUI : MonoBehaviour
{
    [SerializeField] private Item item;

    public string GetItemCost()
    {
        return item.GetBuyPrice().ToString();
    } 
}
