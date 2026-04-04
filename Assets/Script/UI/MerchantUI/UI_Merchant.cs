using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_Merchant : MonoBehaviour
{
    private InventoryPlayer inventory;
    private InventoryMerchant merchant;
    private bool isQuitting;

    [SerializeField] private TextMeshProUGUI goldText;
    [Space]
    [SerializeField] private UI_ItemSlotParent merchantSlots;
    [SerializeField] private UI_ItemSlotParent inventorySlots;
    [SerializeField] private UI_EquipSlotParent equipSlots;

   public void SetUpMerchantUI(InventoryMerchant merchant,InventoryPlayer inventory)
    {
        // 先解绑旧对象事件，避免反复打开商店后重复订阅导致 UI 刷新异常
        UnsubscribeEvents();

        this.merchant = merchant;
        this.inventory = inventory;

        // 防御性判断：当依赖对象为空时不继续绑定，避免空引用
        if (this.merchant == null || this.inventory == null)
            return;

        // 同时监听“玩家背包变化”和“商店库存变化”
        // 玩家背包变化：购买装备/消耗品时会触发
        // 商店库存变化：购买后商店数量减少、补货时会触发
        this.inventory.OnInventoryChange += UpdateSlotUI;
        this.merchant.OnInventoryChange += UpdateSlotUI;
        UpdateSlotUI();
        UI_MerchantSlot[] merchSlots = GetComponentsInChildren<UI_MerchantSlot>();
        foreach (var slot in merchSlots)
        {
            slot.SetUpMerchantUI(merchant);
        }
    }

    private void OnDisable()
    {
        // UI 关闭时解绑，避免悬挂回调和重复订阅
        UnsubscribeEvents();
    }

    private void OnDestroy()
    {
        // 对象销毁时再次解绑，兜底防止退出/切场景时遗留回调
        UnsubscribeEvents();
    }

    private void OnApplicationQuit()
    {
        // 应用退出阶段可能存在销毁顺序问题，打标记后跳过后续 UI 刷新
        isQuitting = true;
        UnsubscribeEvents();
    }

    private void UnsubscribeEvents()
    {
        if (inventory != null)
            inventory.OnInventoryChange -= UpdateSlotUI;

        if (merchant != null)
            merchant.OnInventoryChange -= UpdateSlotUI;
    }

    private void UpdateSlotUI()
    {
        // 退出或对象不完整时直接返回，避免 UI 关闭阶段空引用
        if (isQuitting || merchant == null || inventory == null || merchantSlots == null || inventorySlots == null || equipSlots == null)
            return;

        merchantSlots.UpdateSlots(merchant.itemList);
        inventorySlots.UpdateSlots(inventory.itemList);
        equipSlots.UpdateEquipmentSlots(inventory.equipList);
        goldText.text = inventory.gold.ToString("N0") + "g.";
    }

}
