using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class UI_ToolTip : MonoBehaviour
{
    private RectTransform rect;
    [SerializeField] private Vector2 offset = new Vector2(300, 20);

    
   protected virtual void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public virtual void ShowToolTip(bool show,RectTransform targetRect)
    {
        if (this == null || rect == null)
            return;

        if(show==false)
        {
            rect.position = new Vector2(9999, 9999);
            return;
        }
        UpdatePos(targetRect);
    }



    public void UpdatePos(RectTransform targetRect)
    {
        if (this == null || rect == null || targetRect == null)
            return;

        float screenCenterX = Screen.width / 2f;
        float screenTop = Screen.height;
        float screenBottom = 0;
        Vector2 targetPosition = targetRect.position;

        targetPosition.x=targetPosition.x>=screenCenterX?targetPosition.x-offset.x:targetPosition.x+offset.x;

        float toolTipHalf = rect.sizeDelta.y / 2f;
        float topY=targetPosition.y + toolTipHalf;
       float bottomY=targetPosition.y - toolTipHalf;

        if(topY>screenTop)
        {
            targetPosition.y = screenTop - toolTipHalf - offset.y;
        }
        else if (bottomY<screenBottom)
        {
            targetPosition.y = screenBottom + toolTipHalf + offset.y;
        }

        rect.position = targetPosition;
    }
    public string GetTextColored(string color, string text)
    {
        return $"<color={color}>{text} </color>";
    }
}
