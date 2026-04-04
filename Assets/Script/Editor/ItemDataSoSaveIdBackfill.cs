#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ItemDataSoSaveIdBackfill
{
    static ItemDataSoSaveIdBackfill()
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemDataSo");
        bool changed = false;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemDataSo itemData = AssetDatabase.LoadAssetAtPath<ItemDataSo>(path);

            if (itemData == null)
                continue;

            if (itemData.UpdateSaveId())
                changed = true;
        }

        if (changed)
            AssetDatabase.SaveAssets();
    }
}
#endif
