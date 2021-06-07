using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory Assets", menuName = "Inventory/Inventory Assets")]
public class InventoryAssets : ScriptableObject
{
    public InventoryAsset[] inventoryAssets = null;
}
