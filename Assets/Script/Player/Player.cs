using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : Entity
{
    public static Player instance;
    
    
    
    private bool gameplayInputEnabled = true;
    public UI ui {  get; private set; }

    public PlayerSkillManager skillManager { get; private set;}
    public EntityHealth health {  get; private set;}
    public PlayerVFX playerVFX {  get; private set;}

    public EntityStatusHandller statusHandler { get; private set; }

    public PlayerCombat combat { get; private set; }

    public InventoryPlayer inventory { get; private set;}

    public PlayerStats stats { get; private set; }

    public Player_QuestManager questManager { get; private set; }
    public Player_NpcAffinityManager npcAffinityManager { get; private set; }

    public static event Action OnPlayerDeath;
    [Header("Attack")]
    public Vector2[] attackMovement;
    public Vector2 jumpAttackMovement;
    public float CounterAttackDuration = .2f;
    [Header("Ultimate Ability Details")]
    public float riseSpeed = 25;
    public float riseMaxDistance = 3;

    [Header("Move")]
    public float moveSpeed = 12;
    public float jumpForce;
    public Vector2 wallJumpForce;
   
    [Header("Dash")]
    public float dashSpeed;
    public float QuickDashSpeed;
    public float dashDuration;

    // 当前帧输入快照：所有状态与输入检查都从这里读取。
    public PlayerInputFrame input { get; private set; } = new PlayerInputFrame();

    public Vector2 mousePosition {  get; private set; }

    public float dashDir {  get; private set; }
    

    public bool isBusy { get; private set; }

   
    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    public PlayerJumpState jumpState { get; private set; }

    public PlayerFallState airState { get; private set; }
    public PlayerWallslideState wallslideState { get; private set; }
    public PlayerWallJumpState wallJumpState { get; private set; }
    public PlayerDashState dashState { get; private set; }

    public PlayerPrimaryAttack primaryAttack { get; private set; }

    public PlayerCounterAttackState counterAttack { get; private set; }

    public PlayerJumpAttackState jumpAttack { get; private set; }

    public PlayerDeadState deadState { get; private set; }

    public PlayerSwordThrowState swordThrowState { get; private set; }

    public PlayerDomainExpansionState domainExpansionState { get; private set; }
    protected override void Awake()
    {
        base.Awake();
        
        instance = this;

        ui=FindAnyObjectByType<UI>();
        skillManager=GetComponent<PlayerSkillManager>();
        playerVFX=GetComponent<PlayerVFX>();
        statusHandler = GetComponent<EntityStatusHandller>();
        health = GetComponent<EntityHealth>();
        combat = GetComponent<PlayerCombat>();
        inventory = GetComponent<InventoryPlayer>();
        stats = GetComponent<PlayerStats>();
        questManager = GetComponent<Player_QuestManager>();
        npcAffinityManager = GetComponent<Player_NpcAffinityManager>();

        if (npcAffinityManager == null)
        {
            npcAffinityManager = gameObject.AddComponent<Player_NpcAffinityManager>();
        }

        idleState = new PlayerIdleState(this, stateMachine, "Idle");
        moveState = new PlayerMoveState(this, stateMachine, "Move");
        jumpState = new PlayerJumpState(this, stateMachine, "Jump");
        airState = new PlayerFallState(this, stateMachine, "Jump");
        dashState = new PlayerDashState(this, stateMachine, "Dash");
        wallslideState = new PlayerWallslideState(this, stateMachine, "WallSlide");
        wallJumpState = new PlayerWallJumpState(this, stateMachine, "Jump");
        primaryAttack = new PlayerPrimaryAttack(this, stateMachine, "Attack");
        counterAttack = new PlayerCounterAttackState(this, stateMachine, "CounterAttack");
        jumpAttack = new PlayerJumpAttackState(this, stateMachine, "JumpAttack");
        deadState = new PlayerDeadState(this, stateMachine, "Dead");
        swordThrowState = new PlayerSwordThrowState(this,stateMachine,"SwordThrow");
        domainExpansionState = new PlayerDomainExpansionState(this, stateMachine, "Jump");
    }
    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }


    protected override void Update()
    {
        // 先采集输入，再驱动状态机，确保本帧逻辑读取同一份输入数据。
        if (ui != null)
        {
            SetGameplayInputEnabled(!ui.IsGameplayInputBlocked());
        }
        CaptureInput();
        base.Update();
        CheckInput();
    }

    private void CaptureInput()
    {
        if (!gameplayInputEnabled)
        {
            input.Clear();
            return;
        }
        // 统一采集本帧输入，禁止在业务层重复直接读取 Input。
        input.Capture();
        // 兼容现有依赖 mousePosition 的逻辑（如投掷瞄准）。
        mousePosition = input.mouseScreenPosition;
    }

    private void CheckInput()
    {
        // 全局输入入口：处理不完全隶属于单一状态的输入。
        CheckDashInput();
       
        CheckMagicInput();
        CheckUltimateSkillInput();
        
       
        CheckQuickItemUsed();
    }
    public override void EntityDeath()
    {
        base.EntityDeath();
        OnPlayerDeath?.Invoke();

        stateMachine.ChangeState(deadState);
    }


    protected override IEnumerator SlowDownEntityCo(float duration, float slowMultiplier)
    {
        float originalMoveSpeed = moveSpeed;
        float originalJumpForce = jumpForce;
        float originalAnimSpeed = anim.speed;
        Vector2 originalWallJump = wallJumpForce;
        Vector2 originalJumpAttack = jumpAttackMovement;
        Vector2[] originalAttackVelocity = new Vector2[attackMovement.Length];
        Array.Copy(attackMovement, originalAttackVelocity, attackMovement.Length);

        float speedMultiplier = 1 - slowMultiplier;

        moveSpeed = moveSpeed * speedMultiplier;
        jumpForce = jumpForce * speedMultiplier;
        anim.speed = anim.speed * speedMultiplier;
        wallJumpForce = wallJumpForce * speedMultiplier;
        jumpAttackMovement = jumpAttackMovement * speedMultiplier;

        for (int i = 0; i < attackMovement.Length; i++)
        {
            attackMovement[i] = attackMovement[i] * speedMultiplier;
        }

        yield return new WaitForSeconds(duration);

        moveSpeed = originalMoveSpeed;
        jumpForce = originalJumpForce;
        anim.speed = originalAnimSpeed;
        wallJumpForce = originalWallJump;
        jumpAttackMovement = originalJumpAttack;

        for (int i = 0; i < attackMovement.Length; i++)
        {
            attackMovement[i] = originalAttackVelocity[i];
        }
    }
    private void CheckUltimateSkillInput()
    {
        // 大招输入改为读取输入快照，保持原有行为。
        if (input.ultimatePressed && skillManager.domainExpansion.CanUseSkill())
        {

            if(skillManager.domainExpansion.InstantDomain())
            {
                skillManager.domainExpansion.CreateDomain();
            }
            else
            {
                stateMachine.ChangeState(domainExpansionState);
            }
            skillManager.domainExpansion.SetSkillCooldown();
        }


    }
   
    
    private void CheckQuickItemUsed()
    {
        if (input.quickItem1UsedPressed)
            inventory.TryUseQuickItemInSlot(1);
        if (input.quickItem2UsedPressed)
            inventory.TryUseQuickItemInSlot(2);
           
    }
    private void CheckMagicInput()
    {
        if (input.magicPressed == false)
            return;

        // 按需求保留：F 键同帧尝试触发两个技能，互斥由技能树/升级路径保证。
        skillManager.shard.TryUseSkill();
        skillManager.timeEcho.TryUseSkill();
    }
    
    private void CheckDashInput()
    {
        if (isWallDetected())
        {
            return;
        }
       
        if (input.dashPressed && CanDash())
        {
            skillManager.dash.SetSkillCooldown();
            // 冲刺方向取当前帧横向输入；未输入时回退到朝向方向。
            dashDir = input.moveX;

            if (dashDir == 0)
                dashDir = facingDir;

            stateMachine.ChangeState(dashState);
        }
    }
    public bool CanQuickDash()
    {
        if (skillManager.dash.IsSkillUpgrade() == false)
            return false;
        return true;
    }
    private bool CanDash()
    {
        if (skillManager.dash.OnCooldown())
            return false;

        if (isWallDetected())
            return false;
        if(stateMachine.currentState==dashState||stateMachine.currentState==domainExpansionState)
            return false;

        return true;

    }
    public void SetGameplayInputEnabled(bool enabled)
    {
        gameplayInputEnabled = enabled;
        if (!enabled)
        {
            input.Clear();
        }
    }
    public IEnumerator BusyFor(float _seconds)
    {
        isBusy = true;
        yield return new WaitForSeconds(_seconds);
        isBusy = false;
    }
    public void TeleportPlayer(Vector3 position)=> transform.position = position;
}
