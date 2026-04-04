using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class UI_ItemToolTip :UI_ToolTip
{
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemType;
    [SerializeField] private TextMeshProUGUI itemInfo;

    [SerializeField] private TextMeshProUGUI itemPrice;
    [SerializeField] private Transform merchantInfo;
    [SerializeField] private Transform inventoryInfo;

    public void ShowToolTip(bool show, RectTransform targetRect,InventoryItem itemToShow,bool buyPrice=false,bool showMerchantInfo=false,bool showControls = true) 
    {
        base.ShowToolTip(show, targetRect);
        if (showControls)
        {
            merchantInfo.gameObject.SetActive(showMerchantInfo);
            inventoryInfo.gameObject.SetActive(!showMerchantInfo);
        }
        else
        {
            merchantInfo.gameObject.SetActive(false);
            inventoryInfo.gameObject.SetActive(false);  
        }
        int price = buyPrice ? itemToShow.buyPrice : Mathf.FloorToInt(itemToShow.buyPrice);
        int totalPrice = price * itemToShow.stackSize;

        string fullStackPrice = ($"Price:{price}x,{itemToShow.stackSize}Total:{totalPrice}g");
        string singlePrice = ($"Price:{price}g");

        itemPrice.text = itemToShow.stackSize>1 ? fullStackPrice : singlePrice;
        itemType.text = itemToShow.itemData.itemType.ToString();
        itemInfo.text = itemToShow.getItemInfo();

        string color = getColorByRarity(itemToShow.itemData.itemRarity);

        itemName.text = GetTextColored(color, itemToShow.itemData.itemName);
        
    }
   
    private string getColorByRarity(int rarity)
    {
        if (rarity < 100) return "white";
        if (rarity < 300) return "green";
        if (rarity < 600) return "blue";
        if (rarity < 850) return "purple";
        return "orange";
    }
}
