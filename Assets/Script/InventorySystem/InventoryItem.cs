using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class InventoryItem 
{
    private string itemId;

   public ItemDataSo itemData;
   public int stackSize = 1;

    public ItemModifier[] modifiers { get; private set;}
    public ItemEffect_DataSO itemEffect;

    public int buyPrice {  get; private set;}
    public float sellPrice { get; private set;}


   public InventoryItem(ItemDataSo itemData)
   {
      this.itemData = itemData;

        modifiers = equipmentData()?.modifiers;
        itemEffect= itemData.itemEffect;
        buyPrice = itemData.itemPrice;
        sellPrice=itemData.itemPrice * .35f;


        itemId = itemData.itemName +" - " + Guid.NewGuid();
   }

    public void AddModifier(EntityStats playerStats)
    {
        foreach(var mod in modifiers)
        {
            Stat statToModify = playerStats.getStatByType(mod.statType);
            statToModify.AddModifier(mod.value, itemId);
        }
    }
    public void RemoveModifier(EntityStats playerStats)
    {
        foreach(var mod in modifiers)
        {
            Stat statToModify = playerStats.getStatByType(mod.statType);
            statToModify.RemoveModifier(itemId);
        }
    }
    public void AddItemEffect(Player player)
    {
        if (itemEffect != null)
        {
            itemEffect.Subscribe(player);
        }
    }
    public void RemoveItemEffect(Player player)
    {
        if (itemEffect != null)
        {
            itemEffect.Unsubscribe();
        }
    }

    private EquipmentDataSO equipmentData()
    {
        if (itemData is EquipmentDataSO equipment)
            return equipment;

        return null;
    }


    public bool CanAddStack()=> stackSize < itemData.maxStackSize;
    public void AddStack()
    {
        stackSize++;
    }
    public void RemoveStack()
    {
        stackSize--;
    }
    public string getItemInfo()
    {
        if (itemData.itemType == ItemType.Material)
            return "Used for crafting";

        if (itemData.itemType == ItemType.Consumable)
            return itemData.itemEffect.effectDescription;
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("");

        foreach (var mod in modifiers)
        {
            string modType = GetStatNameByType(mod.statType);
            string modValue = IsPercentageStat(mod.statType) ? mod.value.ToString() + "%" : mod.value.ToString();
            sb.AppendLine("+ " + modValue + " " + modType);
        }

        if (itemEffect != null)
        {
            sb.AppendLine("");
            sb.AppendLine("Unique Effect:");
            sb.AppendLine(itemEffect.effectDescription);

        }


        return sb.ToString();
    }
    private string GetStatNameByType(StatType type)
    {
        switch (type)
        {
            case StatType.MaxHealth: return "Max Health";
            case StatType.HealthRegen: return "Health Regeneration";
            case StatType.Armor: return "Armor";
            case StatType.Evasion: return "Evasion";

            case StatType.Strength: return "Strength";
            case StatType.Agility: return "Agility";
            case StatType.Intelligence: return "Intelligence";
            case StatType.Vitality: return "Vitality";

            case StatType.AttackSpeed: return "Attack Speed";
            case StatType.Damage: return "Damage";
            case StatType.CritChance: return "Critical Chance";
            case StatType.CritPower: return "Critical Power";
            case StatType.ArmorReduction: return "Armor Reduction";

            case StatType.FireDamage: return "Fire Damage";
            case StatType.IceDamage: return "Ice Damage";
            case StatType.LightningDamage: return "Lightning Damage";
            case StatType.ElementalDamage: return "Elemental Damage";

            case StatType.IceResistance: return "Ice Resistance";
            case StatType.FireResistance: return "Fire Resistance";
            case StatType.LightningResistance: return "Lightning Resistance";
            default: return "Unknown Stat";
        }
    }

    private bool IsPercentageStat(StatType type)
    {
        switch (type)
        {
            case StatType.CritChance:
            case StatType.CritPower:
            case StatType.ArmorReduction:
            case StatType.IceResistance:
            case StatType.FireResistance:
            case StatType.LightningResistance:
            case StatType.AttackSpeed:
            case StatType.Evasion:
                return true;
            default:
                return false;
        }
    }
}
