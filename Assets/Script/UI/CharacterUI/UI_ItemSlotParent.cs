using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_ItemSlotParent : MonoBehaviour
{
    private UI_ItemSlot[] slots;

    public void UpdateSlots(List<InventoryItem> itemList)
    {
        // 包含未激活子物体：避免父级 UI 先关闭再打开时缓存到空槽位数组
        // 并且当层级变化时自动重抓，减少“显示和点击对应错位”问题
        if (slots == null || slots.Length == 0)
        {
            slots = GetComponentsInChildren<UI_ItemSlot>(true);
            SortSlotsByVisualOrder();
        }

        if (slots == null || slots.Length == 0)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < itemList.Count)
            {
                slots[i].UpdateSlot(itemList[i]);
            }
            else
            {
                slots[i].UpdateSlot(null);
            }
        }
    }

    private void SortSlotsByVisualOrder()
    {
        // 按视觉顺序排序：先上后下、同一行先左后右，保证“显示第 N 个”与“点击第 N 个”一致
        System.Array.Sort(slots, (a, b) =>
        {
            if (a == null && b == null) return 0;
            if (a == null) return 1;
            if (b == null) return -1;

            RectTransform aRect = a.transform as RectTransform;
            RectTransform bRect = b.transform as RectTransform;
            Vector3 aPos = aRect != null ? aRect.position : a.transform.position;
            Vector3 bPos = bRect != null ? bRect.position : b.transform.position;

            float yDiff = Mathf.Abs(aPos.y - bPos.y);
            if (yDiff > 0.5f)
                return bPos.y.CompareTo(aPos.y);

            return aPos.x.CompareTo(bPos.x);
        });
    }
}
