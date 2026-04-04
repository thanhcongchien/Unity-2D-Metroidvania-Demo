using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_MerchantSlot : UI_ItemSlot
{
    private InventoryMerchant merchant;

    public enum MerchantSlotType
    {
        MerchantSlot,
        PlayerSlot
    }

    public MerchantSlotType slotType;

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (itemInSlot == null)
            return;

        bool rightButton = eventData.button == PointerEventData.InputButton.Right;
        bool leftButton = eventData.button == PointerEventData.InputButton.Left;

        if(slotType == MerchantSlotType.PlayerSlot)
        {
            if(rightButton)
            {
                bool sellFullstack = Input.GetKey(KeyCode.LeftControl);
                merchant.TrySellItem(itemInSlot, sellFullstack);
            }
            else if(leftButton)
            {
                base.OnPointerDown(eventData);
            }
        }
        else if(slotType == MerchantSlotType.MerchantSlot)
        {
           if(leftButton)
                return;

            bool buyFullstack = Input.GetKey(KeyCode.LeftControl);
            merchant.TryBuyItem(itemInSlot, buyFullstack);
        }
        ui.itemToolTip.ShowToolTip(false, null);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if(itemInSlot==null) return;

        if(slotType == MerchantSlotType.MerchantSlot)
        {
            ui.itemToolTip.ShowToolTip(true, rect, itemInSlot,true,true);
        }
        else
            ui.itemToolTip.ShowToolTip(true, rect, itemInSlot,false,true);
    }
    public void SetUpMerchantUI(InventoryMerchant merchant) => this.merchant = merchant;
}
