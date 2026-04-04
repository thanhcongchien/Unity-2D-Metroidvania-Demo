using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_CraftPreviewSlot : MonoBehaviour
{
    [SerializeField] private Image materialIcon;
    [SerializeField] private TextMeshProUGUI materialName;

    public void SetUpMaterialSlot(ItemDataSo itemData,int availableAmount,int requiredAmount)
    {
        materialIcon.sprite = itemData.itemIcon;
        materialName.text = itemData.itemName + " - " + availableAmount + "/" + requiredAmount;
    }    
}
