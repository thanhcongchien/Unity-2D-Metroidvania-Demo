using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryMerchant : InventoryBase
{
    private InventoryPlayer inventory;

    [SerializeField] private ItemListDataSO shopData;
    
    [SerializeField] private int minItemsAmount = 4;
    [SerializeField] private int maxItemsAmount = 10;

    protected override void Awake()
    {
        base.Awake();
        FillShopList();
    }

    public void TryBuyItem(InventoryItem itemToBuy,bool buyFullStack)
    {
        int amountToBuy = buyFullStack ? itemToBuy.stackSize : 1;
        for (int i = 0; i < amountToBuy; i++)
        {
            if(inventory.gold<itemToBuy.buyPrice)
            {
                Debug.Log("Not enough gold");
                return;
             }
            bool purchaseSucceeded = false;

            if(itemToBuy.itemData.itemType==ItemType.Material)
            {
                // 材料购买时创建新实例，避免商店与仓库引用同一个 InventoryItem 导致数量联动错误
                var itemToAdd = new InventoryItem(itemToBuy.itemData);
                inventory.storage.AddMaterialToStash(itemToAdd);
                purchaseSucceeded = true;
            }
            else
            {
                if(inventory.CanAddItem(itemToBuy))
                {
                    var itemToAdd = new InventoryItem(itemToBuy.itemData);
                    inventory.AddItem(itemToAdd);
                    purchaseSucceeded = true;
                }
            }

            // 只有在“实际成功加入玩家侧容器”后，才扣钱并减少商店库存，避免白扣金币/白扣商店物品
            if (purchaseSucceeded == false)
            {
                Debug.Log("Not enough inventory space");
                return;
            }

            inventory.gold-=itemToBuy.buyPrice;
            RemoveOneItem(itemToBuy);
        }
        TriggerUpdateUI();
    }

    public void TrySellItem(InventoryItem itemToSell,bool sellFullStack)
    {
        int amountToSell = sellFullStack ? itemToSell.stackSize : 1;
        for(int i = 0;i < amountToSell;i++)
        {
            int sellPrice = Mathf.FloorToInt(itemToSell.sellPrice);

            inventory.gold+=sellPrice;
            inventory.RemoveOneItem(itemToSell);
        }

        TriggerUpdateUI();
    }
    public void FillShopList()
    {

        itemList.Clear();
        List<InventoryItem> possibleItems = new List<InventoryItem>();

        foreach(var itemData in shopData.itemList)
        { 
            int randomoziedStack=Random.Range(itemData.minStackSizeAtShop,itemData.maxStackSizeAtShop + 1);
            int finalStack=Mathf.Clamp(randomoziedStack,1,itemData.maxStackSize);

            InventoryItem itemToAdd = new InventoryItem(itemData);

            itemToAdd.stackSize = finalStack;
            possibleItems.Add(itemToAdd);
        }

        int randomItemsAmount = Random.Range(minItemsAmount, maxItemsAmount + 1);
        int finalAmount = Mathf.Clamp(randomItemsAmount, 1,possibleItems.Count);

       for(int i = 0; i < finalAmount; i++) 
        {
            var index = Random.Range(0, possibleItems.Count);
            var item = possibleItems[index];

            if(CanAddItem(item))
            {
                possibleItems.Remove(item);
                AddItem(item);
            }
        
        }

       TriggerUpdateUI();
    }



    public void SetInventory(InventoryPlayer inventory)
    {
        this.inventory = inventory;
    }
}
