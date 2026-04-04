using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CraftPreview : MonoBehaviour
{
    private InventoryItem itemToCraft;
    private InventoryStorage storage;
    private UI_CraftPreviewSlot[] craftRecipeSlots;

    [Header("Item preview Setup")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemInfo;
    [SerializeField] private TextMeshProUGUI buttonText;




    public void SetUpCraftPreview(InventoryStorage storage)
    { 
        this.storage = storage;
        craftRecipeSlots = GetComponentsInChildren<UI_CraftPreviewSlot>(true);

        foreach(var slot in craftRecipeSlots)
        {
            slot.gameObject.SetActive(false);
        }
    }
    public void ConfirmCraft()
    {
        if(itemToCraft == null)
        {
            buttonText.text = "Pick an item";
            return;
        }
        if(storage.HasEnoughMaterials(itemToCraft)&& storage.inventory.CanAddItem(itemToCraft))
        { 

            storage.ConsumeMaterials(itemToCraft);
            // 每次制作都创建新实例，避免把同一个对象引用重复塞进背包
            InventoryItem craftedItem = new InventoryItem(itemToCraft.itemData);
            storage.inventory.AddItem(craftedItem);
        }
        UpdateCraftPreviewSlots();
    }

    public void UpdateCraftPreview(ItemDataSo itemData)
    {
        itemToCraft = new InventoryItem(itemData);

        itemIcon.sprite = itemData.itemIcon;
        itemName.text = itemData.itemName;
        itemInfo.text = itemToCraft.getItemInfo();
        UpdateCraftPreviewSlots();
    }

    private void UpdateCraftPreviewSlots()
    {
        if (itemToCraft == null || itemToCraft.itemData == null || craftRecipeSlots == null || craftRecipeSlots.Length == 0)
            return;

        foreach (var slot in craftRecipeSlots)
        {
            slot.gameObject.SetActive(false);
        }

        int recipeCount = itemToCraft.itemData.craftRecipe == null ? 0 : itemToCraft.itemData.craftRecipe.Length;
        int slotsToUse = Mathf.Min(recipeCount, craftRecipeSlots.Length);

        if (recipeCount > craftRecipeSlots.Length)
        {
            Debug.LogWarning($"UI_CraftPreview '{name}' has {recipeCount} recipe items but only {craftRecipeSlots.Length} preview slots.");
        }

        for (int i = 0; i < slotsToUse; i++)
        {
            InventoryItem requiredItem = itemToCraft.itemData.craftRecipe[i];
            if (requiredItem == null || requiredItem.itemData == null)
                continue;

            int avaliableAmount = storage.getAvaliableAmountOf(requiredItem.itemData);

            craftRecipeSlots[i].gameObject.SetActive(true);
            craftRecipeSlots[i].SetUpMaterialSlot(requiredItem.itemData, avaliableAmount, requiredItem.stackSize);
        }
    }
}
