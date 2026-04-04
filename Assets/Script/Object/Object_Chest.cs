using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour, IDamagable, ISaveable
{
    private Rigidbody2D rb;
    private EntityVFX fx;
    private Entity_DropManager dropManager;
    private Animator animator;

    [SerializeField] private bool canDropItems = true;

    private string chestSaveId;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        fx = GetComponent<EntityVFX>();
        dropManager = GetComponent<Entity_DropManager>();
        animator = GetComponentInChildren<Animator>();
        chestSaveId = BuildSaveId();
    }

    public bool takeDamage(float damage,float elementalDamage,ElementType element, Transform damageDealer)
    {
        if(!canDropItems) return false;

        ApplyOpenedState(dropItems: true, playFeedback: true);

        if (SaveManager.instance != null)
            SaveManager.instance.SaveGame();

        return true;

    }

    private void ApplyOpenedState(bool dropItems, bool playFeedback)
    {
        canDropItems = false;

        if (dropItems)
            dropManager.DropItems();

        animator.SetBool("Open", true);

        if (playFeedback)
        {
            fx.PlayOnDamageVfx();
            rb.velocity = new Vector2(0, 5);
            rb.angularVelocity = Random.Range(-150f, 150f);
        }
        else
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private string BuildSaveId()
    {
        List<string> hierarchyPath = new List<string>();
        Transform current = transform;

        while (current != null)
        {
            hierarchyPath.Add($"{current.GetSiblingIndex()}:{current.name}");
            current = current.parent;
        }

        hierarchyPath.Reverse();
        return $"{gameObject.scene.name}/{string.Join("/", hierarchyPath)}";
    }

    private void EnsureChestSaveData(GameData data)
    {
        if (data.openedChests == null)
            data.openedChests = new SerializableDictionary<string, bool>();
    }

    public void LoadData(GameData data)
    {
        EnsureChestSaveData(data);

        if (data.openedChests.TryGetValue(chestSaveId, out bool opened) && opened)
            ApplyOpenedState(dropItems: false, playFeedback: false);
    }

    public void SaveData(ref GameData data)
    {
        EnsureChestSaveData(data);

        if (canDropItems == false)
            data.openedChests[chestSaveId] = true;
        else
            data.openedChests.Remove(chestSaveId);
    }
}
