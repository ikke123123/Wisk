using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory Asset", menuName = "Inventory/Inventory Asset")]
public class InventoryAsset : ScriptableObject
{
    [Tooltip("e.g. iron <= try to not use capital letters, but it shouldn't really matter if you're consistent.")] public string assetId = "";
    [Tooltip("e.g. Iron <= capitalize this, or make it visually appealing. Used in the inventory screen.")] public string assetName = "";
    [Tooltip("e.g. Units of Iron, or Scroll of the Dead <= can be singular, or plural, depending on whether you thing you'll have one or multiple. Used in the \"Received thing\" message box.")] public string assetUnits = "";
    public Sprite assetIcon = null;
    public TypeOf typeOf = TypeOf.Valuable;

    public enum TypeOf { Valuable, Resource, QuestItem, Consumable };
}
