using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_StorageSlot : UI_ItemSlot
{
    private InventoryStorage storage;
    public enum StorageType { StorageSlot,PlayerInventorySlot}
    public StorageType slotType;

    public void SetStorage(InventoryStorage storage) => this.storage = storage;
    public override void OnPointerDown(PointerEventData eventData)
    {
        if(itemInSlot == null) return;

        bool transferFullStack = Input.GetKey(KeyCode.LeftControl);


        if(slotType == StorageType.StorageSlot)
        {
            storage.FromStorageToPlayer(itemInSlot, transferFullStack);
        }
        if (slotType == StorageType.PlayerInventorySlot)
        {
            storage.FromPlayerToStorage(itemInSlot, transferFullStack);
        }
        ui.itemToolTip.ShowToolTip(false, null);
    }
}
