using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_DropManager : Entity_DropManager
{
    [Range(0,100)]
    [SerializeField] private float chanceToLooseItem = 90f;
    private InventoryPlayer inventory;

    private void Awake()
    {
        inventory = GetComponent<InventoryPlayer>();
    }

    public override void DropItems()
    {
        List<InventoryItem> inventoryCopt = new List<InventoryItem>(inventory.itemList);
        List<InventoryEquipmentSlot> equipCopy = new List<InventoryEquipmentSlot>(inventory.equipList);

        foreach(var item  in inventoryCopt)
        {
            if(Random.Range(0,100)<chanceToLooseItem)
            {
                CreateItemDrop(item.itemData);
                inventory.RemoveAllItem(item);
            }
        }
    }


}
