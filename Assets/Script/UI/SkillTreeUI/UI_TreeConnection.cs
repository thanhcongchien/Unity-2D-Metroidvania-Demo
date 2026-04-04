using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_TreeConnection : MonoBehaviour
{
    [SerializeField] private RectTransform rotationPoint;
    [SerializeField] private RectTransform connectionLength;
    [SerializeField] private RectTransform childNodeConnectionPoint;

    public void DirectionConnection(NodeDirectionType direction, float length,float offset)
    {
        bool shouldBeActive = direction != NodeDirectionType.None;
        float finalLength = shouldBeActive ? length : 0f;
        float angle = GetDirectionAngle(direction);

        rotationPoint.localRotation = Quaternion.Euler(0f, 0f, angle+offset);
        connectionLength.sizeDelta = new Vector2(finalLength, connectionLength.sizeDelta.y);
    }

    public Image getConnectionImage()=>connectionLength.GetComponent<Image>();


    public Vector2 getConnectionPoint(RectTransform rect)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle
            (
            rect.parent as RectTransform,
            childNodeConnectionPoint.position,
            null,
            out var localPosition
            );
            return localPosition;
    }


   private float GetDirectionAngle(NodeDirectionType type)
    {
        switch (type)
        {
            case NodeDirectionType.UpLeft: return 180f;
            case NodeDirectionType.Up: return 135f;
            case NodeDirectionType.UpRight: return 90f;
            case NodeDirectionType.Left: return -135f;
            case NodeDirectionType.Right: return 45f;
            case NodeDirectionType.DownLeft: return -90f;
            case NodeDirectionType.Down: return -45;
            case NodeDirectionType.DownRight: return -0f;
            default: return 0f;
        }
    }

   
}
public enum NodeDirectionType
{
    None,
    UpLeft,
    Up,
    UpRight,
    Left,
    Right,
    DownLeft,
    Down,
    DownRight

}
