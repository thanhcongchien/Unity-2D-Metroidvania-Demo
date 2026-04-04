using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Entity_DropManager : MonoBehaviour
{
    [SerializeField] private GameObject itemDropPrefab;
    [SerializeField] private ItemListDataSO dropData;

    [Header("Drop restrictions")]
    [SerializeField] private int maxRarityAmount = 1200;
    [SerializeField] private int maxItemsToDrop = 3;

    [Header("Drop spawn")]
    [SerializeField] private float spawnHorizontalSpread = .35f;
    [SerializeField] private float spawnVerticalOffset = .35f;
    [SerializeField] private float spawnResolveStep = .2f;
    [SerializeField] private float spawnCheckRadius = .18f;
    [SerializeField] private int spawnResolveAttempts = 6;
    [SerializeField] private LayerMask groundLayer;

    private Collider2D sourceCollider;

    private void Awake()
    {
        EnsureSpawnDefaults();
        sourceCollider = GetComponent<Collider2D>();

        if (sourceCollider == null)
            sourceCollider = GetComponentInChildren<Collider2D>();

        ResolveGroundLayerMask();
    }

    private void OnValidate()
    {
        EnsureSpawnDefaults();
        ResolveGroundLayerMask();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            DropItems();
        }
    }

    public virtual void DropItems()
    {
        if(dropData == null)
        {
            Debug.Log("No drop data set");
            return;
        }

        List<ItemDataSo> itemToDrop = RollDrops();
        int amountToDrop = Mathf.Min(itemToDrop.Count, maxItemsToDrop);

        for(int i = 0; i < amountToDrop; i++)
        {
            CreateItemDrop(itemToDrop[i]);
        }
    }
    public void CreateItemDrop(ItemDataSo itemToDrop)
    {
        Vector2 spawnPosition = GetDropSpawnPosition();
        GameObject newItem = Instantiate(itemDropPrefab, spawnPosition, Quaternion.identity);
        newItem.GetComponent<Object_ItemPickUp>().SetupItem(itemToDrop);

    }
    
    public List<ItemDataSo> RollDrops()
    {
        List<ItemDataSo> possibleDrops = new List<ItemDataSo>();
        List<ItemDataSo> finalDrops = new List<ItemDataSo>();

        float maxRarityAmount = this.maxRarityAmount;

        foreach(var item in dropData.itemList)
        {
            float dropChance = item.getDropChance();

            if(Random.Range(0,100) < dropChance)
                possibleDrops.Add(item);
        }

        possibleDrops = possibleDrops.OrderByDescending(item => item.itemRarity).ToList();

        foreach(var item in possibleDrops)
        { 
            if(maxRarityAmount>item.itemRarity)
            {
                finalDrops.Add(item);
                maxRarityAmount -= item.itemRarity;
            }
        }
        return finalDrops;
    }

    private Vector2 GetDropSpawnPosition()
    {
        Bounds sourceBounds = sourceCollider != null
            ? sourceCollider.bounds
            : new Bounds(transform.position, Vector3.one);

        float horizontalSpread = Mathf.Max(spawnHorizontalSpread, sourceBounds.extents.x * .35f);
        Vector2 basePosition = new Vector2(sourceBounds.center.x, sourceBounds.max.y + spawnVerticalOffset);

        for (int i = 0; i < Mathf.Max(1, spawnResolveAttempts); i++)
        {
            Vector2 candidate = basePosition
                + new Vector2(Random.Range(-horizontalSpread, horizontalSpread), i * spawnResolveStep);

            if (IsSpawnPositionClear(candidate))
                return candidate;
        }

        return basePosition + Vector2.up * (spawnResolveAttempts * spawnResolveStep);
    }

    private bool IsSpawnPositionClear(Vector2 position)
    {
        if (groundLayer.value == 0)
            return true;

        return Physics2D.OverlapCircle(position, spawnCheckRadius, groundLayer) == null;
    }

    private void ResolveGroundLayerMask()
    {
        if (groundLayer.value != 0)
            return;

        int groundLayerIndex = LayerMask.NameToLayer("Ground");
        if (groundLayerIndex != -1)
            groundLayer = 1 << groundLayerIndex;
    }

    private void EnsureSpawnDefaults()
    {
        spawnHorizontalSpread = Mathf.Max(.1f, spawnHorizontalSpread);
        spawnVerticalOffset = Mathf.Max(.1f, spawnVerticalOffset);
        spawnResolveStep = Mathf.Max(.05f, spawnResolveStep);
        spawnCheckRadius = Mathf.Max(.05f, spawnCheckRadius);
        spawnResolveAttempts = Mathf.Max(1, spawnResolveAttempts);
    }
}
