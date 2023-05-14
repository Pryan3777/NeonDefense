using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] protected int buyPrice;
    [SerializeField] protected int sellPrice;


    public virtual int GetBuyPrice()
    {
        return buyPrice;
    }

    public virtual int GetSellPrice()
    {
        return sellPrice;
    }
}
