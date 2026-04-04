using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Craft : MonoBehaviour
{
    [SerializeField] private UI_ItemSlotParent inventoryParent;
    private InventoryPlayer inventory;

    private UI_CraftPreview craftPreviewUI;
    private UI_CraftSlot[] craftSlots;
    private UI_CraftListButton[] craftListButtons;

   

    public void SetUpCraftUI(InventoryStorage storage)
    {
        // 切换到新 inventory 前先解绑旧监听，避免重复刷新
        if (inventory != null)
            inventory.OnInventoryChange -= UpdateUI;

        inventory = storage.inventory;
        // 保险做法：先 -= 再 +=，保证只注册一次
        inventory.OnInventoryChange -= UpdateUI;
        inventory.OnInventoryChange += UpdateUI;
        UpdateUI();

        craftPreviewUI = GetComponentInChildren<UI_CraftPreview>();
        craftPreviewUI.SetUpCraftPreview(storage);
        SetUpCraftListButtons();
    }
    private void SetUpCraftListButtons()
    {
        craftSlots = GetComponentsInChildren<UI_CraftSlot>(true);

        foreach(var slot in craftSlots)
        { 
            slot.gameObject.SetActive(false);
        }
        craftListButtons = GetComponentsInChildren<UI_CraftListButton>(true);

        foreach(var button in craftListButtons)
        {
            button.SetCraftSlots(craftSlots);
        }

    }
    private void UpdateUI()
    {
        inventoryParent.UpdateSlots(inventory.itemList);
    }
}
