using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object_ItemPickUp : MonoBehaviour
{
   [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D col;
    [Space]
    [SerializeField] private ItemDataSo itemData;
    [SerializeField] private Vector2 dropForce=new Vector2(3, 10);
    [SerializeField] private float settleSurfacePadding = .05f;
    [SerializeField] private float spawnResolveStep = .15f;
    [SerializeField] private int spawnResolveAttempts = 8;
    [SerializeField] private LayerMask groundLayer;

    

    private void OnValidate()
    {
        EnsureRuntimeDefaults();

        if (itemData == null)
            return;


        sr = GetComponent<SpriteRenderer>();
        SetUpVisuals();
        ResolveGroundLayerMask();

    }

    public void SetupItem(ItemDataSo itemData)
    {
        EnsureRuntimeDefaults();
        this.itemData = itemData;
        SetUpVisuals();
        ResolveGroundLayerMask();
        ResolveInitialGroundOverlap();

        rb.constraints = RigidbodyConstraints2D.None;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.angularVelocity = 0f;

        float xForce = Random.Range(-dropForce.x, dropForce.x);
        rb.velocity = new Vector2(xForce, dropForce.y);
        col.isTrigger = false;

    }

    private void SetUpVisuals()
    {
        sr.sprite = itemData.itemIcon;
        gameObject.name = "Object_ItemPickup - " + itemData.itemName;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TrySettleOnGround(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TrySettleOnGround(collision);
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        InventoryPlayer inventory = collision.GetComponent<InventoryPlayer>();

        if (inventory == null)
            return;
        InventoryItem itemToAdd=new InventoryItem(itemData);
        InventoryStorage storage = inventory.storage;
        
        if(itemData.itemType==ItemType.Material)
        {
            storage.AddMaterialToStash(itemToAdd);
            Destroy(gameObject);
            return;
        }

        

        if(inventory.CanAddItem(itemToAdd))
        {
            inventory.AddItem(itemToAdd);
            Destroy(gameObject);
        }
    }

    private void TrySettleOnGround(Collision2D collision)
    {
        if (collision == null || col == null || rb == null || col.isTrigger)
            return;

        if (IsGroundLayer(collision.gameObject.layer) == false)
            return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);

            if (contact.normal.y < .35f)
                continue;

            float halfHeight = col.bounds.extents.y;
            Vector3 settledPosition = transform.position;
            settledPosition.y = Mathf.Max(settledPosition.y, contact.point.y + halfHeight + settleSurfacePadding);
            transform.position = settledPosition;
            Physics2D.SyncTransforms();

            col.isTrigger = true;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            return;
        }
    }

    private void ResolveInitialGroundOverlap()
    {
        if (col == null)
            return;

        Physics2D.SyncTransforms();

        for (int i = 0; i < spawnResolveAttempts; i++)
        {
            if (IsOverlappingGround() == false)
                return;

            transform.position += Vector3.up * spawnResolveStep;
            Physics2D.SyncTransforms();
        }
    }

    private bool IsOverlappingGround()
    {
        if (groundLayer.value == 0 || col == null)
            return false;

        Bounds bounds = col.bounds;
        Vector2 size = new Vector2(Mathf.Max(.05f, bounds.size.x * .9f), Mathf.Max(.05f, bounds.size.y * .9f));
        return Physics2D.OverlapBox(bounds.center, size, 0f, groundLayer) != null;
    }

    private bool IsGroundLayer(int layer)
    {
        return (groundLayer.value & (1 << layer)) != 0;
    }

    private void ResolveGroundLayerMask()
    {
        if (groundLayer.value != 0)
            return;

        int groundLayerIndex = LayerMask.NameToLayer("Ground");
        if (groundLayerIndex != -1)
            groundLayer = 1 << groundLayerIndex;
    }

    private void EnsureRuntimeDefaults()
    {
        settleSurfacePadding = Mathf.Max(.01f, settleSurfacePadding);
        spawnResolveStep = Mathf.Max(.05f, spawnResolveStep);
        spawnResolveAttempts = Mathf.Max(1, spawnResolveAttempts);
    }
}
