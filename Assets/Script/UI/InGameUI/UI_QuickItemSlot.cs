using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_QuickItemSlot : UI_ItemSlot
{
    private Button button;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private int slotNumber;
    public int SlotNumber => slotNumber;

    protected override void Awake()
    {
        base.Awake();
        button = GetComponent<Button>();
    }


    public void SetupQuickSlotItem(InventoryItem itemToPass)
    {
        inventoryPlayer.SetQuickItemInSlot(slotNumber, itemToPass);
    }
    public void SimulateButtonFeedback()
    {
        EventSystem.current.SetSelectedGameObject(button.gameObject);   
        ExecuteEvents.Execute(button.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);

    }



    public void UpdateQuickSlotUI(InventoryItem currentItemInSlot)
    {
       
        if (currentItemInSlot == null || currentItemInSlot.itemData == null)
        {
            itemIcon.sprite = defaultSprite;
            itemStackSize.text = "";
            return;
        }
        itemIcon.sprite = currentItemInSlot.itemData.itemIcon;
        itemStackSize.text = currentItemInSlot.stackSize.ToString();

    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        ui.inGameUI.OpenQuickItemOptions(this, rect);
    }
}
