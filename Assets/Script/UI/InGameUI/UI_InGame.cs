using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Rendering;

public class UI_InGame : MonoBehaviour
{
     private Player player;
     private UI_SkilSlot[] skillSlots;
    private InventoryPlayer inventory;
    
    [SerializeField] private RectTransform healthRect;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Quick Item Slots")]
    [SerializeField] private float yOffsetQuickItemParent = 150;
    [SerializeField] private Transform quickItemOptionsParent;
    private UI_QuickItemSlotOption[] quickItemOptions;
    private UI_QuickItemSlot[] quickItemSlots;

    private void Start()
    {
        quickItemSlots = GetComponentsInChildren<UI_QuickItemSlot>(true);

        player = FindFirstObjectByType<Player>();
        player.health.OnHealthUpdate += UpadteHealthBar;
        inventory = player.inventory;

        skillSlots = GetComponentsInChildren<UI_SkilSlot>(true);
        // 回退到旧流程，只监听快捷槽位事件，不走背包全量刷新。
        inventory.OnQuickSlotUsed += UpdateQuickSlotsUI;
        // 背包变化时补一次快捷栏同步，保证商店购买后数量即时更新。
        inventory.OnInventoryChange += RefreshQuickSlotsFromInventory;
        RefreshQuickSlotsFromInventory();
     }
    public void UpdateQuickSlotsUI(int slotNumber,InventoryItem itemInSlot)
    {
        if (quickItemSlots == null || quickItemSlots.Length == 0)
        {
            return;
        }

        // 事件里的 slotNumber 是 0 基，UI 组件序列化的 SlotNumber 是 1 基。
        int targetSlotNumber = slotNumber + 1;

        // 按槽位编号匹配，避免 GetComponentsInChildren 返回顺序导致错位更新。
        for (int i = 0; i < quickItemSlots.Length; i++)
        {
            UI_QuickItemSlot slot = quickItemSlots[i];
            if (slot != null && slot.SlotNumber == targetSlotNumber)
            {
                slot.UpdateQuickSlotUI(itemInSlot);
                return;
            }
        }

        if (slotNumber < 0 || slotNumber >= quickItemSlots.Length)
        {
            return;
        }

        // 兜底逻辑，避免老场景未配置 slotNumber 时完全不更新。
        quickItemSlots[slotNumber].UpdateQuickSlotUI(itemInSlot);
    }

    private void RefreshQuickSlotsFromInventory()
    {
        if (inventory == null)
        {
            return;
        }

        inventory.RefreshQuickSlotsUI();
    }


    public void OpenQuickItemOptions(UI_QuickItemSlot quickItemSlot,RectTransform targetRect)
    {
        // 未绑定选项父节点时直接返回, 避免空引用.
        if (quickItemOptionsParent == null)
        {
            Debug.LogWarning("UI_InGame: quickItemOptionsParent is null.");
            return;
        }

        if(quickItemOptions == null)
            quickItemOptions = quickItemOptionsParent.GetComponentsInChildren<UI_QuickItemSlotOption>(true);

        List<InventoryItem> consumables=inventory.itemList.FindAll(item=>item.itemData.itemType == ItemType.Consumable);

        for(int i = 0; i < quickItemOptions.Length; i++)
        {
            if(i<consumables.Count)
            {
                quickItemOptions[i].gameObject.SetActive(true);
                quickItemOptions[i].SetupOption(quickItemSlot,consumables[i]);
            }
            else
            {
                quickItemOptions[i].gameObject.SetActive(false);
            }
        }
        quickItemOptionsParent.position = targetRect.position + Vector3.up * yOffsetQuickItemParent;
    }

    public void HideQuickItemOptions()
    {
        // 未绑定选项父节点时不执行隐藏逻辑.
        if (quickItemOptionsParent == null)
        { 
            Debug.Log("UI_InGame: quickItemOptionsParent is null.");
            return;
        }

        quickItemOptionsParent.position = new Vector3(0,9999);
    }


    public UI_SkilSlot getSkillSlot(SkillType skillType)
    {
        foreach(var slot in skillSlots)
        {
            if (slot.skillType == skillType)
            { 
                slot.gameObject.SetActive(true);
                return slot;
            }
        }
        return null;
    }

    private void UpadteHealthBar()
    {
        float currentHealth = Mathf.RoundToInt(player.health.getCurrentHp());
        float maxHealth = player.stats.getMaxHealth();
        float sizeDifference = Mathf.Abs(maxHealth - healthRect.sizeDelta.x);

        if(sizeDifference > 0.1f)
                healthRect.sizeDelta = new Vector2(maxHealth, healthRect.sizeDelta.y);


        healthText.text = currentHealth + "/" + maxHealth;
        healthSlider.value = player.health.getHealthPercent();

    }

}


