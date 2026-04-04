using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using JetBrains.Annotations;

public class InventoryPlayer : InventoryBase
{
    public event Action<int, InventoryItem> OnQuickSlotUsed;
    public int gold = 100000;
    
    
    public List<InventoryEquipmentSlot> equipList;
    public InventoryStorage storage {  get; private set; }

    [Header("Quick Item Slots")]
    // 快捷栏改为运行时数据，不读取场景序列化，避免“拿到药水就自动装备”。
    private InventoryItem[] quickItems = new InventoryItem[2];

    protected override void Awake()
    {
        base.Awake(); 
       
        storage = FindFirstObjectByType<InventoryStorage>();

    }

    public void SetQuickItemInSlot(int slotNumber, InventoryItem itemToSet)
    {
        quickItems[slotNumber - 1] = itemToSet;
        // 回退到旧流程，快捷栏仅更新被修改的槽位。
        OnQuickSlotUsed?.Invoke(slotNumber - 1, itemToSet);
    }

    public void TryUseQuickItemInSlot(int slotNumber)
    {
        var itemToUse = quickItems[slotNumber - 1];
        if(itemToUse == null)
            return;
        TryUseItem(itemToUse);

        if(FindItem(itemToUse)==null)
        {
            quickItems[slotNumber - 1] = FindSameItem(itemToUse);
        }

        // 回退到旧流程，使用后把当前槽位最新物品状态推给 UI。
        OnQuickSlotUsed?.Invoke(slotNumber - 1, quickItems[slotNumber - 1]);
    }

    public void RefreshQuickSlotsUI()
    {
        // 当背包数量变化时，仅同步已手动装备槽位的引用与数量，不做自动装备。
        for (int i = 0; i < quickItems.Length; i++)
        {
            if (quickItems[i] != null && FindItem(quickItems[i]) == null)
            {
                quickItems[i] = FindSameItem(quickItems[i]);
            }

            OnQuickSlotUsed?.Invoke(i, quickItems[i]);
        }
    }

    public void TryEquipItem(InventoryItem item)
    {
       var inventoryItem = FindItem(item);
        var matchingSlots = equipList.FindAll(slot => slot.slotType == item.itemData.itemType);

       foreach (var slot in matchingSlots)
        {
            if(slot.HasItem()==false)
            {
                EquipItem(inventoryItem, slot);
                return;
            }
        }

       var slotToReplace = matchingSlots[0];
        var itemToUnequip = slotToReplace.equipedItem;


        UnequipItem(itemToUnequip,slotToReplace != null);
        EquipItem(inventoryItem, slotToReplace);




    }
    public void EquipItem(InventoryItem itemToEquip, InventoryEquipmentSlot slot)
    {
        float savedHealthPercent=player.health.getHealthPercent();


        slot.equipedItem = itemToEquip;
        slot.equipedItem.AddModifier(player.stats);
        slot.equipedItem.AddItemEffect(player);
        player.health.SetHealthToPercent(savedHealthPercent);

        RemoveOneItem(itemToEquip);
    }
    public void UnequipItem(InventoryItem itemToUnequip,bool replacingItem=false)
    {
        if(CanAddItem(itemToUnequip)==false && replacingItem==false) 
        {
            
            return;
        }
        float savedHealthPercent = player.health.getHealthPercent();

       var slotToUnequip = equipList.Find(slot => slot.equipedItem == itemToUnequip);

        if(slotToUnequip!=null)
            slotToUnequip.equipedItem = null;


        itemToUnequip.RemoveModifier(player.stats);
        itemToUnequip.RemoveItemEffect(player);
        player.health.SetHealthToPercent(savedHealthPercent);
        AddItem(itemToUnequip);
    }

    public override void SaveData(ref GameData data)
    {
        data.gold = gold;
        data.inventory.Clear();
        data.equipedItems.Clear();
        foreach(var item in itemList)
        {
            if(item!=null&&item.itemData!=null)
            {
                string saveId = item.itemData.saveID;

                if (string.IsNullOrWhiteSpace(saveId))
                {
                    Debug.LogWarning($"Skip saving inventory item `{item.itemData.itemName}` because saveID is empty.");
                    continue;
                }

                if (data.inventory.ContainsKey(saveId) == false)
                    data.inventory[saveId] = 0;

                data.inventory[saveId] += item.stackSize;
            }
        }
        foreach(var slot in equipList)
        {
            if(slot.HasItem())
            {
                string saveId = slot.equipedItem.itemData.saveID;

                if (string.IsNullOrWhiteSpace(saveId))
                {
                    Debug.LogWarning($"Skip saving equipped item `{slot.equipedItem.itemData.itemName}` because saveID is empty.");
                    continue;
                }

                data.equipedItems[saveId] = slot.slotType;
            }
        }
    }

    public override void LoadData(GameData data)
    {
        if (HasValidItemDataBase() == false)
            return;

        gold = data.gold;
        float savedHealthPercent = player != null && player.health != null
            ? player.health.getHealthPercent()
            : 1f;

        foreach (var entry in data.inventory)
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
        foreach (var entry in data.equipedItems)
        {
            string saveId = entry.Key;
            ItemType loadSlotType = entry.Value;

            ItemDataSo itemData = itemDataBase.GetItemData(saveId);
            InventoryItem itemToLoad = new InventoryItem(itemData);


            var slot = equipList.Find(slot => slot.slotType == loadSlotType && slot.HasItem() == false);

            slot.equipedItem = itemToLoad;
            slot.equipedItem.AddModifier(player.stats);
            slot.equipedItem.AddItemEffect(player);
        }

        if (player != null && player.health != null)
            player.health.SetHealthToPercent(savedHealthPercent);

        TriggerUpdateUI();
    }
}

