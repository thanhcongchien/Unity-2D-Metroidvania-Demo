using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item list", fileName = "List of items - ")]
public class ItemListDataSO :ScriptableObject
{
    public ItemDataSo[] itemList;

    public ItemDataSo GetItemData(string saveId)
    {
        return itemList.FirstOrDefault(item => item != null && item.saveID == saveId);
    }

#if UNITY_EDITOR
    [ContextMenu("Auto-fill with all ItemDataSO")]
    public void CollectItemsData()
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemDataSo");

        itemList = guids.Select(guid => AssetDatabase.LoadAssetAtPath<ItemDataSo>(AssetDatabase.GUIDToAssetPath(guid))).Where(item=>item!=null).ToArray();

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

    }
#endif
}
