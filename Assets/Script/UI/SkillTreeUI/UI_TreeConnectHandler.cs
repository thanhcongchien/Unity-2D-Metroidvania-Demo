using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class UI_TreeConnectDetails
{
    public UI_TreeConnectHandler childNode;
    public NodeDirectionType direction;
   [Range(100f,350f)] public float length;
    [Range(-25f,25f)] public float rotation;
}


public class UI_TreeConnectHandler : MonoBehaviour
{
    private RectTransform rect=>GetComponent<RectTransform>();
    [SerializeField] private UI_TreeConnectDetails[] ConnectionDetails;
    [SerializeField] private UI_TreeConnection[] connections;

    private Image connectionImage;
    private Color originalColor;
    private bool hasOriginalColor;

    private void Awake()
    {
        if (connectionImage != null)
        {
            originalColor = connectionImage.color;
            hasOriginalColor = true;
        }
    }

    public UI_TreeNode[] getChildNodes()
    {
        List<UI_TreeNode> childrenToReturn = new List<UI_TreeNode>();
        foreach (var node in ConnectionDetails)
        {
            if (node.childNode != null)
                childrenToReturn.Add(node.childNode.GetComponent<UI_TreeNode>());
        }


        return childrenToReturn.ToArray();
    }






    private void OnValidate()
    {
        if (ConnectionDetails.Length <= 0)
            return;


        if (ConnectionDetails.Length != connections.Length)
        {
            Debug.Log("长度不一致");
            return;
        }
        UpdateConnections(false);
    }


    private void UpdateConnections(bool reorderSiblings = true)
    {
        for(int i = 0;i<ConnectionDetails.Length;i++)
        {
            var detail = ConnectionDetails[i];
            var connection = connections[i];
            if(connection==null) continue;
            connection.DirectionConnection(detail.direction,detail.length,detail.rotation);

            Vector2 targetPosition = connection.getConnectionPoint(rect);
            Image connectionImage = connection.getConnectionImage();


            if (detail.childNode == null)
                continue;

            detail.childNode.SetPosition(targetPosition);
            detail.childNode.SetConnectionImage(connectionImage);

            if (reorderSiblings)
                detail.childNode.transform.SetAsLastSibling();
        }
    }

    public void UpdateAllConnections()
    {
        UpdateConnections();
        foreach (var node in ConnectionDetails)
        {
            if(node.childNode==null)
                continue;
           node.childNode.UpdateConnections();
        }

    }
    public void ConnectionImageUnlock(bool unlocked)
    {
        if(connectionImage==null) return;

        if (!hasOriginalColor)
        {
            originalColor = connectionImage.color;
            hasOriginalColor = true;
        }

        connectionImage.color = unlocked ? Color.white : originalColor;
    }
    public void SetConnectionImage(Image image)
    {
        connectionImage = image;
        if (connectionImage != null && !hasOriginalColor)
        {
            originalColor = connectionImage.color;
            hasOriginalColor = true;
        }
    }
    public void SetPosition(Vector2 position)=>rect.anchoredPosition = position;
}

