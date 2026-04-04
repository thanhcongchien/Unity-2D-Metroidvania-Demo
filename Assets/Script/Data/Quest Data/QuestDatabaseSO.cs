using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "RPG Setup/Quest Data/Quest Database", fileName = "QUEST DATABASE")]
public class QuestDatabaseSO : ScriptableObject
{
    public QuestDataSO[] allQuests;

    public QuestDataSO GetQuestById(string id)
    {
        return allQuests.FirstOrDefault(q => q != null && q.questSaveID == id);

    }
     

#if UNITY_EDITOR
    [ContextMenu("Auto-fill with all QuestDataSO")]
    public void CollectQuestData()
    {
        string[] guids = AssetDatabase.FindAssets("t:QuestDataSO");

        allQuests = guids.Select(guid => AssetDatabase.LoadAssetAtPath<QuestDataSO>(AssetDatabase.GUIDToAssetPath(guid))).Where(q => q != null).ToArray();

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

    }
#endif
}
