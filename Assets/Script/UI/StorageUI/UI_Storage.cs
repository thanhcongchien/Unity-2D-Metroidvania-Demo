using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Storage : MonoBehaviour
{
    private InventoryStorage storage;
    private InventoryPlayer inventory;

    [SerializeField] private UI_ItemSlotParent inventoryParent;
   [SerializeField] private UI_ItemSlotParent storageParent;
    [SerializeField] private UI_ItemSlotParent materialParent;

    private void OnEnable()
    {
        UpdateUI();
    }


    public void SetUpStorageUI(InventoryStorage storage)
    {
        

        // 先解绑旧 storage，防止反复打开 UI 时重复订阅
        if (this.storage != null)
            this.storage.OnInventoryChange -= UpdateUI;

        this.storage = storage;
        inventory = storage.inventory;
        // 再次保险解绑后绑定，确保只有一份监听
        storage.OnInventoryChange -= UpdateUI;
        storage.OnInventoryChange += UpdateUI;
        UpdateUI();

        UI_StorageSlot[] storageSlots = GetComponentsInChildren<UI_StorageSlot>();

        foreach(var slot in storageSlots)
        {
            slot.SetStorage(storage);
        }
    }

    private void UpdateUI()
    {
        if (storage == null)
            return;
        inventoryParent.UpdateSlots(inventory.itemList);
        storageParent.UpdateSlots(storage.itemList);
        materialParent.UpdateSlots(storage.materialStash);
    }
}
