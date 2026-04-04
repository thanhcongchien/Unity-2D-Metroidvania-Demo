using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_CraftListButton : MonoBehaviour
{
    [SerializeField] private ItemListDataSO craftData;
    private UI_CraftSlot[] craftSlots;

    public void SetCraftSlots(UI_CraftSlot[] craftSlots) => this.craftSlots = craftSlots;


    public void UpdateCraftSlots()
    {
        if (craftData == null || craftSlots == null || craftSlots.Length == 0)
            return;

        foreach(var slot in craftSlots)
        {
            slot.gameObject.SetActive(false);
        }

        int itemCount = craftData.itemList == null ? 0 : craftData.itemList.Length;
        int slotsToUse = Mathf.Min(itemCount, craftSlots.Length);

        if (itemCount > craftSlots.Length)
        {
            Debug.LogWarning($"UI_CraftListButton '{name}' has {itemCount} craft items but only {craftSlots.Length} craft slots.");
        }

        for (int i = 0; i < slotsToUse; i++)
        {
            ItemDataSo itemData = craftData.itemList[i];

            if (itemData == null)
                continue;

            craftSlots[i].gameObject.SetActive(true);
            craftSlots[i].SetUpButton(itemData);
        }
    }

}
