using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_QuestRewardSlot : UI_ItemSlot
{
    public override void OnPointerDown(PointerEventData eventData)
    {
        
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);


        ui.itemToolTip.ShowToolTip(true, rect, itemInSlot, false, false, false);
    }
}
