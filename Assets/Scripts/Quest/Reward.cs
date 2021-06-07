using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Reward
{
    public Reward(int quantity)
    {
        if (quantity < 1)
            throw new ArgumentOutOfRangeException();

        this.quantity = quantity;
        typeOf = TypeOf.XP;
        id = "";
    }

    public Reward(string id, int quantity)
    {
        if (quantity < 1)
            throw new ArgumentOutOfRangeException();

        typeOf = TypeOf.Item;
        this.id = id;
        this.quantity = quantity;
    }

    public void GiveReward()
    {
        if (typeOf == TypeOf.Item)
        {
            InventoryManager.iM.Add(id, quantity);
        }
        //Implement XP Solution.
        else XPManager.xPM.Add(quantity);
    }

    public TypeOf typeOf;
    public string id;
    public int quantity;

    public enum TypeOf { XP, Item };
}
