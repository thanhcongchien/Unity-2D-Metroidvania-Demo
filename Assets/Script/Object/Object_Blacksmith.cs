using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object_Blacksmith : Object_NPC, IInteractable
{
    [Header("Quest & Dialogue")]
    [SerializeField] private DialogueLineSO firstDialogueLine;
    [SerializeField] private DialogueLineSO[] priorityDialogueLines;
    [SerializeField] private QuestDataSO[] quests;

    private Animator anim;
    private InventoryStorage storage;
    private InventoryPlayer inventory;
    
    protected override void Awake()
    {
        base.Awake();
        storage = GetComponent<InventoryStorage>();
        anim=GetComponentInChildren<Animator>();
        anim.SetBool("isBlacksmith", true);
    }


    public override void Interact()
    {
        base.Interact();
        if (inventory == null && Player.instance != null)
            inventory = Player.instance.inventory;

        storage.SetInventory(inventory);
        ui.storageUI.SetUpStorageUI(storage);
        ui.craftUI.SetUpCraftUI(storage);

        DialogueLineSO selectedLine = ResolveDialogueLine(firstDialogueLine, priorityDialogueLines, inventory);
        if (selectedLine == null)
            return;

        ui.ToggleDialogueUI(selectedLine, CreateDialogueNpcData(quests));
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        inventory = player.GetComponent<InventoryPlayer>();
        storage.SetInventory(inventory);
    }
    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);
        ui.SwitchOffAllToolTips();
        //ui.storageUI.gameObject.SetActive(false);
        //ui.craftUI.gameObject.SetActive(false);
    }
}
