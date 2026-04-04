using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_Inventory : MonoBehaviour
{
   
    private InventoryPlayer inventory;
    

    [SerializeField] private UI_ItemSlotParent inventorySlotsParent;
    [SerializeField] private UI_EquipSlotParent equipSlotParent;
    [SerializeField] private TextMeshProUGUI goldText;

    private void Awake()
    {
       
        inventory = FindFirstObjectByType<InventoryPlayer>();
        inventory.OnInventoryChange += UpdateUI;


        UpdateUI();
    }

    private void OnEnable()
    {
        if (inventory == null)
            return;

        UpdateUI();
    }
    public void UpdateUI()
    {
       inventorySlotsParent.UpdateSlots(inventory.itemList);
        equipSlotParent.UpdateEquipmentSlots(inventory.equipList);
        goldText.text = inventory.gold.ToString("N0") + "g.";
    }


  


    
   
    
}
