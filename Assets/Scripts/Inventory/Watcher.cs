using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Watcher
{
    //ADD CALLBACK TO THE MISSION WHERE THIS THING COMES FROM, FOR NOW I'LL JUST PUT IN A SIMPLE ID MARKER
    public Watcher(string questElementID, Comparison comparison/*, string inventoryID*/, int value)
    {
        this.questElementID = questElementID;
        this.comparison = comparison;
        //this.inventoryID = inventoryID;
        this.value = value;
    }

    public bool CheckIfPassed(int currentQuantity)
    {
        bool output = false;
        switch ((int)comparison)
        {
            case 0:
                output = currentQuantity > value;
                break;
            case 1:
                output = currentQuantity >= value;
                break;
            case 2:
                output = currentQuantity < value;
                break;
            case 3:
                output = currentQuantity <= value;
                break;
            case 4:
                output = currentQuantity == value;
                break;
            case 5:
                output = currentQuantity != value;
                break;
        }

        if (output) Execute();

        return output;
    }

    private void Execute()
    {
        //Add callback to mission element.
        throw new NotImplementedException();
    }

    private string questElementID;
    private Comparison comparison;
    //private string inventoryID = "";
    private int value;

    public enum Comparison { GeaterThan, GreaterThanOrEqualTo, LessThan, LessThanOrEqualTo, EqualTo, NotEqualTo };
}
