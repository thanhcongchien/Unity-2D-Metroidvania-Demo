using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryBase : MonoBehaviour,ISaveable
{
    protected Player player;
    public event Action OnInventoryChange;

    public int maxInventorySize = 10;
    public List<InventoryItem> itemList = new List<InventoryItem>();

    [Header("ITEM DATA BASE")]
    [SerializeField] protected ItemListDataSO itemDataBase;

    protected virtual void Awake()
    {
        player = GetComponent<Player>();
    }




    public bool CanAddItem(InventoryItem itemToAdd)
    {
        bool hasStackable = FindStackable(itemToAdd) != null;
        return hasStackable|| itemList.Count < maxInventorySize;
    }
    public InventoryItem FindStackable(InventoryItem itemToAdd)
    {
      return itemList.Find(item => item.itemData == itemToAdd.itemData&&item.CanAddStack());
       
    }
    
    public void TryUseItem(InventoryItem itemToUse)
    {
        InventoryItem consumable=itemList.Find(item => item == itemToUse);

        if(consumable==null) { return; }
        if (consumable.itemEffect.CanBeUsed(player) == false)
            return;

        consumable.itemEffect.ExecuteEffect();

        if(consumable.stackSize>1)
            consumable.RemoveStack();
        else
           RemoveOneItem(consumable);

        OnInventoryChange?.Invoke();
    }
    public void AddItem(InventoryItem itemToAdd)
    {
        
        InventoryItem itemInInventory = FindStackable(itemToAdd);
        if(itemInInventory != null)
        {
            itemInInventory.AddStack();
        }
        else
        {
            
            itemList.Add(itemToAdd);
        }

        OnInventoryChange?.Invoke();
    }
    public void RemoveOneItem(InventoryItem itemToRemove)
    {
        InventoryItem itemInInventory=itemList.Find(item => item == itemToRemove);
        // 保护性判断：当目标已被移除或引用不在当前容器时，直接返回，避免空引用异常
        if (itemInInventory == null)
            return;

        if(itemInInventory.stackSize>1)
             itemInInventory.RemoveStack();
        else
            itemList.Remove(itemInInventory);

        OnInventoryChange?.Invoke();

    }

    public void RemoveAllItem(InventoryItem itemToRemove)
    {
        for(int i = 0; i < itemToRemove.stackSize; i++)
        {
            RemoveOneItem(itemToRemove);

        }
    }

    public void RemoveItemAmount(ItemDataSo itemToRemove, int amount)
    {
        for(int i = 0; i < itemList.Count; i++)
        {
            InventoryItem item = itemList[i];

            if(item.itemData != itemToRemove)
                continue;

            int removeCount = Mathf.Min(item.stackSize, amount);

            for(int j = 0; j < removeCount; j++)
            {
                RemoveOneItem(item);
                amount--;

                if(amount <= 0)
                    break;
            }
        }
    }

    public bool HasItemAmount(ItemDataSo itemToCheck,int  amount)
    {
        int total = 0;
        foreach(var item in itemList)
        {
            if(item.itemData == itemToCheck)
            {
                total += item.stackSize;
            }
            if(total >= amount)
            {
                return true;
            }
        }
        return false;
    }

    public InventoryItem FindItem(InventoryItem itemToFind)
    {
        return  itemList.Find(item => item == itemToFind);
    }

    public InventoryItem FindSameItem(InventoryItem itemToFind)
    {
        return itemList.Find(item => item.itemData == itemToFind.itemData);
    }
    public void TriggerUpdateUI() => OnInventoryChange?.Invoke();

    protected bool HasValidItemDataBase()
    {
        if (itemDataBase != null)
            return true;

        Debug.LogError($"{GetType().Name} on `{gameObject.name}` is missing `{nameof(itemDataBase)}` reference.");
        return false;
    }

    public virtual void LoadData(GameData data)
    {
       
    }

    public virtual void SaveData(ref GameData data)
    {
        
    }
}
