using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ItemSlot : MonoBehaviour,IPointerDownHandler,IPointerEnterHandler,IPointerExitHandler
{
    [Header("UI Slot Setup")]
    [SerializeField] protected Image itemIcon;
    [SerializeField] protected TextMeshProUGUI itemStackSize;

    public InventoryItem itemInSlot {  get; private set; }
    public InventoryPlayer inventoryPlayer {  get; private set; }

    protected UI ui;

    protected RectTransform rect;

    protected virtual void Awake()
    {
        ui = GetComponentInParent<UI>();
        rect = GetComponent<RectTransform>();
        inventoryPlayer = FindFirstObjectByType<InventoryPlayer>();
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if(itemInSlot==null||itemInSlot.itemData.itemType==ItemType.Material) return;

        bool alternativeInput = Input.GetKey(KeyCode.LeftControl);

        if (alternativeInput)
        {
            inventoryPlayer.RemoveOneItem(itemInSlot);
        }
        else
        {
            if (itemInSlot.itemData.itemType == ItemType.Consumable)
            {
               

                inventoryPlayer.TryUseItem(itemInSlot);
            }
            else
                inventoryPlayer.TryEquipItem(itemInSlot);
        }
        if(itemInSlot==null)
            ui.itemToolTip.ShowToolTip(false, null);
    }
    
    public void UpdateSlot(InventoryItem item)
    {
        itemInSlot = item;
        if(itemInSlot == null)
        {
           
            itemStackSize.text = "";
           
            itemIcon.color= Color.clear;
            return;
        }

        Color color =Color.white;
        color.a = .9f;
        itemIcon.color = color;
        itemIcon.sprite = itemInSlot.itemData.itemIcon;
        itemStackSize.text = item.stackSize > 1 ? itemInSlot.stackSize.ToString() : "";
    }
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
       if(itemInSlot==null)
            return;

       ui.itemToolTip.ShowToolTip(true, rect, itemInSlot);

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ui.itemToolTip.ShowToolTip(false, null);
    }

}
