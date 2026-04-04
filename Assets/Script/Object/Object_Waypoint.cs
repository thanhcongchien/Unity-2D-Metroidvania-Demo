using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class Object_Waypoint : MonoBehaviour
{

    [SerializeField] private string transferToScene;
    [Space]
    [SerializeField] private RespawnType waypointType;
    [SerializeField] private RespawnType connectedWaypoint;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private bool canBeTriggered = true;

    
    public RespawnType GetWaypointType()=> waypointType;

    public Vector3 GetPositionAndSetTriggerFalse()
    {
        canBeTriggered = false;
        return respawnPoint == null ? transform.position : respawnPoint.position;
    }


    private void OnValidate()
    {
        if(waypointType == RespawnType.Enter)
            connectedWaypoint = RespawnType.Exit;

        if(waypointType == RespawnType.Exit)
            connectedWaypoint = RespawnType.Enter;

        gameObject.name = "Object_Waypoint - " + waypointType + " -> " + connectedWaypoint + " - " + transferToScene;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!canBeTriggered) { return; }

        Player player = collision.GetComponentInParent<Player>();
        if (player == null)
            return;

        GameManager.Instance.ChangeScene(transferToScene,connectedWaypoint);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        canBeTriggered = true;
    }
}
