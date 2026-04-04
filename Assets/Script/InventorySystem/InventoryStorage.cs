using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryStorage : InventoryBase
{
    public InventoryPlayer inventory {  get; private set; }
    public List<InventoryItem> materialStash;

   
    
    public void ConsumeMaterials(InventoryItem itemToCraft)
    {
        foreach(var requiredMaterial in itemToCraft.itemData.craftRecipe)
        {
            int amountToConsume = requiredMaterial.stackSize;
           amountToConsume-= ConsumedMaterialsAmount(inventory.itemList,requiredMaterial);
           if(amountToConsume > 0 )
            {
                amountToConsume-= ConsumedMaterialsAmount(itemList,requiredMaterial);
            }
           if(amountToConsume > 0)
            {
                amountToConsume -= ConsumedMaterialsAmount(materialStash,requiredMaterial);
            }
        }

        // 材料消耗后主动通知 UI 刷新，确保仓库/材料栏数量立即更新
        TriggerUpdateUI();
    }


    private int ConsumedMaterialsAmount(List<InventoryItem> itemList,InventoryItem neededItem)
    {
        int amountNeeded = neededItem.stackSize;
        int consumedAmount = 0;

        // 倒序遍历 + RemoveAt，避免在 foreach 中删除元素导致集合修改异常
        for (int i = itemList.Count - 1; i >= 0; i--)
        {
            InventoryItem item = itemList[i];

            if(item.itemData != neededItem.itemData)
                continue;

            int removeAmount = Mathf.Min(item.stackSize, amountNeeded - consumedAmount);
            item.stackSize -= removeAmount;
            consumedAmount += removeAmount;

            if(item.stackSize <= 0)
                itemList.RemoveAt(i);

            // 达到需求数量就停止，避免多扣材料
            if(consumedAmount >= amountNeeded)
                break;
        }
        return consumedAmount;
    }

    public bool HasEnoughMaterials(InventoryItem itemToCraft)
    {
        foreach(var requiredMaterial in itemToCraft.itemData.craftRecipe)
        {
            if(getAvaliableAmountOf(requiredMaterial.itemData) < requiredMaterial.stackSize)
            {
                return false;
            }
        }
        return true;
    }



    public int getAvaliableAmountOf(ItemDataSo requiredItem)
    {
        int amount = 0;

        foreach(var item in inventory.itemList)
        {
            if(item.itemData == requiredItem)
            {
                amount += item.stackSize;
            }
        }
        foreach(var item in itemList)
        {
            if(item.itemData == requiredItem)
            {
                amount += item.stackSize;
            }

        }
        foreach(var item in materialStash)
        {
            if(item.itemData == requiredItem)
            {
                amount += item.stackSize;
            }
        }
        return amount;
    }


    public void AddMaterialToStash(InventoryItem itemToAdd)
    {
        var stackableItem = StackableInStash(itemToAdd);

        if (stackableItem != null)
            stackableItem.AddStack();
        else
        {
            var newitemtoAdd = new InventoryItem(itemToAdd.itemData);
            materialStash.Add(newitemtoAdd);
        }

        TriggerUpdateUI();
        materialStash = materialStash.OrderBy(item => item.itemData.itemName).ToList();
    }

    public InventoryItem StackableInStash(InventoryItem itemToAdd)
    {
       return  materialStash.Find(item => item.itemData == itemToAdd.itemData&&item.CanAddStack());

       
    }

    public void SetInventory(InventoryPlayer inventory)=>this.inventory = inventory;

    public void FromPlayerToStorage(InventoryItem item,bool transferFullStack)
    {
        int transferStackSize = transferFullStack ? item.stackSize : 1;
        for (int i = 0; i < transferStackSize; i++)
        {



            if (CanAddItem(item))
            {
                var itemToAdd = new InventoryItem(item.itemData);
                inventory.RemoveOneItem(item);
                AddItem(itemToAdd);
            }
        }
        TriggerUpdateUI();
    }

    public void FromStorageToPlayer(InventoryItem item, bool transferFullStack)
    {
        int transferStackSize = transferFullStack ? item.stackSize : 1;
        for (int i = 0; i < transferStackSize; i++)
        {
            if (inventory.CanAddItem(item))
            {
                var itemToAdd = new InventoryItem(item.itemData);

                RemoveOneItem(item);
                inventory.AddItem(itemToAdd);
            }
        }
        TriggerUpdateUI();
    }
    public override void SaveData(ref GameData data)
    {
        base.SaveData(ref data);

        data.storageItems.Clear();
        foreach (var item in itemList)
        {
            if (item != null && item.itemData != null)
            {
                string saveId = item.itemData.saveID;

                if (string.IsNullOrWhiteSpace(saveId))
                {
                    Debug.LogWarning($"Skip saving storage item `{item.itemData.itemName}` because saveID is empty.");
                    continue;
                }

                if (data.storageItems.ContainsKey(saveId) == false)
                    data.storageItems[saveId] = 0;

                data.storageItems[saveId] += item.stackSize;
            }
        }

        data.storageMaterials.Clear();
        foreach (var item in materialStash)
        {
            if (item != null && item.itemData != null)
            {
                string saveId = item.itemData.saveID;

                if (string.IsNullOrWhiteSpace(saveId))
                {
                    Debug.LogWarning($"Skip saving storage material `{item.itemData.itemName}` because saveID is empty.");
                    continue;
                }

                if (data.storageMaterials.ContainsKey(saveId) == false)
                    data.storageMaterials[saveId] = 0;

                data.storageMaterials[saveId] += item.stackSize;
            }
        }
    }
    public override void LoadData(GameData data)
    {
        if (HasValidItemDataBase() == false)
            return;

        if (materialStash == null)
            materialStash = new List<InventoryItem>();

       itemList.Clear();
        materialStash.Clear();
        foreach (var entry in data.storageItems)
        {
            string saveId = entry.Key;
            int stackSize = entry.Value;

            ItemDataSo itemData = itemDataBase.GetItemData(saveId);

            if (itemData == null)
            {
                Debug.LogWarning("No item data found for save ID: " + saveId);
                continue;
            }


            for (int i = 0; i < stackSize; i++)
            {
                InventoryItem itemToLoad = new InventoryItem(itemData);
                AddItem(itemToLoad);
            }
        }
        foreach (var entry in data.storageMaterials)
        {
            string saveId = entry.Key;
            int stackSize = entry.Value;

            ItemDataSo itemData = itemDataBase.GetItemData(saveId);

            if (itemData == null)
            {
                Debug.LogWarning("No item data found for save ID: " + saveId);
                continue;
            }


            for (int i = 0; i < stackSize; i++)
            {
                InventoryItem itemToLoad = new InventoryItem(itemData);
                var stackableItem = StackableInStash(itemToLoad);
                if (stackableItem != null)
                    stackableItem.AddStack();
                else
                    materialStash.Add(itemToLoad);
            }
        }

        materialStash = materialStash.OrderBy(item => item.itemData.itemName).ToList();
        TriggerUpdateUI();
    }
}
