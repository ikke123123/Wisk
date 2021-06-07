using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager iM = null;

    [SerializeField] private InventoryAssets inventoryAssets = null;

    [Header("Debug")]
    [SerializeField] private bool save = true;
    [SerializeField] private List<TempInventory> inventory = null;

    private Dictionary<string, TempInventory> idToInventoryItem = null;

    private void Awake()
    {
        if (iM != null && iM != this)
        {
            Destroy(iM);
        }
        iM = this;
    }

    private void Start()
    {
        inventory = new List<TempInventory>();
        idToInventoryItem = new Dictionary<string, TempInventory>();
        LoadTempInventory();
    }






    public void Add(string id, int quantity)
    {
        if (quantity < 1)
            throw new ArgumentOutOfRangeException();
        OnValueChanged(id, quantity);
    }

    public void Subtract(string id, int quantity)
    {
        if (quantity < 1)
            throw new ArgumentOutOfRangeException();
        OnValueChanged(id, -1 * quantity);
    }

    public int GetQuantity(string id)
    {
        if (!idToInventoryItem.ContainsKey(id))
            throw new NullReferenceException();

        return idToInventoryItem[id].quantity;
    }

    public void AssignWatcher(Watcher watcher, string id)
    {
        if (!idToInventoryItem.ContainsKey(id))
            throw new NullReferenceException();

        TempInventory tempInventory = idToInventoryItem[id];

        tempInventory.watchers.Add(watcher);
        CheckWatchers(tempInventory);
    }

    public void RemoveWatcher(Watcher watcher, string id)
    {
        if (!idToInventoryItem.ContainsKey(id))
            throw new NullReferenceException();

        TempInventory tempInventory = idToInventoryItem[id];
        tempInventory.watchers.Remove(watcher);
    }






    private void OnValueChanged(string id, int quantity)
    {
        if (!idToInventoryItem.ContainsKey(id))
            throw new NullReferenceException();

        TempInventory tempInventory = idToInventoryItem[id];
        tempInventory.quantity += quantity;

        //Check for watchers
        CheckWatchers(tempInventory);

        //Set new saved value.
        if (save)
            PlayerPrefs.SetInt(id, tempInventory.quantity);

        //Send chat message about received units.
        MessageManager.SendMessage(MessageManager.TypeOf.Inventory, string.Format("{0} {1} received!", quantity, tempInventory.inventoryAsset.assetUnits));
    }

    //Startup check etc.
    private void LoadTempInventory()
    {
        foreach (InventoryAsset _inventoryAsset in inventoryAssets.inventoryAssets)
        {
            TempInventory tempInventory = new TempInventory
            {
                inventoryAsset = _inventoryAsset,
                quantity = save ? PlayerPrefs.GetInt(_inventoryAsset.assetId, 0) : 0
            };
            inventory.Add(tempInventory);
            //Allows us to find it back later using the id.
            idToInventoryItem.Add(_inventoryAsset.assetId, tempInventory);

            CheckWatchers(tempInventory);
        }
    }

    private void CheckWatchers(TempInventory tempInventory)
    {
        foreach (Watcher watcher in tempInventory.watchers)
        {
            watcher.CheckIfPassed(tempInventory.quantity);
        }
    }

    [Serializable]
    private class TempInventory
    {
        public InventoryAsset inventoryAsset = null;
        public List<Watcher> watchers = new List<Watcher>();
        public int quantity = 0;
    }
}