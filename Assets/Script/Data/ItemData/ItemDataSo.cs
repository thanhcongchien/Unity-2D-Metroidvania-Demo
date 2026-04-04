using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Material item", fileName = "Material data - ")]
public class ItemDataSo : ScriptableObject
{
    [SerializeField] private string saveId;
    public string saveID => saveId;


    [Header("Merchant Details")]
    [Range(0,10000)]
    public int itemPrice = 100;
    public int minStackSizeAtShop = 1;
    public int maxStackSizeAtShop = 1;

    [Header("Drop Details")]
    [Range(0,1000)]
    public int itemRarity = 100;
    [Range(0,100)]
    public float dropChance = 100;

    [Range(0, 100)]
    public float maxDropChance = 65f;

    [Header("Item Details")]
    public string itemName;
    public Sprite itemIcon;
    public ItemType itemType;
    public int maxStackSize = 1;

    [Header("ItemEffect")]
    public ItemEffect_DataSO itemEffect;

    [Header("Craft Details")]
    public InventoryItem[] craftRecipe;

    private void OnValidate()
    {
        dropChance = getDropChance();

#if UNITY_EDITOR
        UpdateSaveId();
#endif
    }

#if UNITY_EDITOR
    public bool UpdateSaveId()
    {
        string path = AssetDatabase.GetAssetPath(this);
        string guid = string.IsNullOrWhiteSpace(path) ? string.Empty : AssetDatabase.AssetPathToGUID(path);

        if (saveId == guid)
            return false;

        saveId = guid;
        EditorUtility.SetDirty(this);
        return true;
    }
#endif

    public float getDropChance()
    {
        float maxRarity = 1000;
        float chance = (maxRarity - itemRarity + 1) / maxRarity * 100;

        return Mathf.Min(chance, maxDropChance);
    }

}
