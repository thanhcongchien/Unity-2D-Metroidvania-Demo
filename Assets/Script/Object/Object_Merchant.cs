using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object_Merchant : Object_NPC, IInteractable
{
    [Header("Quest & Dialogue")]
    [SerializeField] private DialogueLineSO firstDialogueLine;
    [SerializeField] private DialogueLineSO[] priorityDialogueLines;
    [SerializeField] private QuestDataSO[] quests;

    private InventoryPlayer inventory;
    private InventoryMerchant merchant;

    protected override void Awake()
    {
        base.Awake();
        merchant = GetComponent<InventoryMerchant>();
    }

    protected override void Update()
    {
        base.Update();
        if(Input.GetKeyDown(KeyCode.Z)) 
        {
            merchant.FillShopList();
        }

    }

    public override void Interact()
    {
        base.Interact();
        if (inventory == null && Player.instance != null)
            inventory = Player.instance.inventory;

        ui.merchantUI.SetUpMerchantUI(merchant, inventory);

        DialogueLineSO selectedLine = ResolveDialogueLine(firstDialogueLine, priorityDialogueLines, inventory);
        if (selectedLine == null)
            return;

        ui.ToggleDialogueUI(selectedLine, CreateDialogueNpcData(quests));



        //ui.ToggleQuestUI(quests);

        //ui.merchantUI.gameObject.SetActive(true);
        //// 先激活 UI 再初始化，确保槽位组件都可被正确获取并绑定数据
       



    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        inventory = player.GetComponent<InventoryPlayer>();
        merchant.SetInventory(inventory);

    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);
        ui.SwitchOffAllToolTips();
        
    }

}
