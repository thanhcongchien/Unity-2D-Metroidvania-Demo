using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class Enemy_Reaper : Enemy,ICounterable
{
    public bool CanBeCountered { get => canBeStunned; }

    public Enemy_ReaperAttackState reaperAttackState { get; private set;}
    public Enemy_ReaperTeleportState reaperTeleportState { get; private set;}
    public Enemy_ReaperBattleState reaperBattleState { get; private set;}
    public Enemy_ReaperPhaseShiftState reaperPhaseShiftState { get; private set; }
    public Enemy_ReaperSpellCastState reaperSpellCastState { get; private set; }

    [Header("Reaper specifics")]
    [SerializeField] public float maxBattleIdleTime = 5f;
    [SerializeField] private bool requireDamageToStartBattle = true;
    [Header("Reaper Spellcast")]
    [SerializeField] private DamageScaleData spellDamageScale;
    [SerializeField] private GameObject spellcastPrefab;
    [SerializeField] private int amountToCast = 6;
    [SerializeField] private float spellCastRate = 1.2f;
    [SerializeField] private float spellCooldown = 10;
    [SerializeField] private Vector2 playerOffsetPrediction;
    [SerializeField] private float spellPredictionTime = .35f;
    [SerializeField] private float minPredictVelocity = .25f;
    [SerializeField] private float maxHorizontalPrediction = 3f;
    private float lastTimeCastSpell = float.NegativeInfinity;
    public bool spellCastPerformed { get; private set; }
    private Player playerScript;

    [Header("Phase 2")]
    [SerializeField][Range(0.05f, 1f)] private float phase2TriggerHealthPercent = .5f;
    [SerializeField] private bool disableTeleportInPhase1 = true;
    [SerializeField] private Color phase2TintColor = new Color(1f, .55f, .55f, 1f);
    [SerializeField] private float phase2AttackCooldownMultiplier = .65f;
    [SerializeField] private float phase2BattleMoveSpeedMultiplier = 1.25f;
    [SerializeField] private float phase2SpellCooldownMultiplier = .7f;
    [SerializeField] private float phase2SpellCastRateMultiplier = .8f;
    [SerializeField] private int phase2AdditionalSpellsPerCast = 2;
    [SerializeField] private float phase2TeleportChanceBonus = .2f;
    [SerializeField] private float phase2AttackSpeedBonus = .35f;

    [Header("Phase Shift Presentation")]
    [SerializeField] private int phaseShiftTeleportCount = 5;
    [SerializeField] private float phaseShiftTeleportInterval = .05f;
    [SerializeField] private float phaseShiftEndDelay = .2f;
    [SerializeField] private float phaseShiftBehindOffset = 1.4f;
    [SerializeField] private float phaseShiftFadeOutDuration = .2f;
    [SerializeField] private float phaseShiftBlackScreenDuration = 2f;
    [SerializeField] private float phaseShiftFadeInDuration = .35f;
    [SerializeField] private string phaseShiftBackgroundRootName = "BG,BackGround";
    [SerializeField] private Color phaseShiftBackgroundColor = Color.black;

    [Header("Boss Music")]
    [SerializeField] private string bossMusicGroupName = "Boss_Music";

    private const string Phase2AttackSpeedModifierSource = "Reaper_Phase2";
    private bool phase2Triggered;
    private bool phaseShiftInProgress;
    private bool phaseShiftBackgroundApplied;
    private bool battleUnlocked;
    private SpriteRenderer reaperSprite;
    private Color defaultSpriteColor = Color.white;
    private UI_FadeScreen phaseShiftFadeScreen;
    private SpriteRenderer[] phaseShiftBackgroundRenderers;
    private Tilemap[] phaseShiftBackgroundTilemaps;


    [Header("Reapert Teleport")]
    [SerializeField] private BoxCollider2D arenaBounds;
    [SerializeField] private float offsetCenterY = 2.5f;
    [SerializeField] private float chanceToTeleport = 0.25f;
    private float defaultTeleportChance;

    public bool teleportTrigger {  get; private set; }

    protected override void Awake()
    {
        base.Awake();

        idleState = new EnemyIdleState(this, stateMachine, "Idle");
        moveState = new EnemyMoveState(this, stateMachine, "Move");
        stunnedState = new EnemyStunnedState(this, stateMachine, "Stun");
        deadState = new EnemyDeadState(this, stateMachine, "Idle"); ;

        reaperBattleState = new Enemy_ReaperBattleState(this, stateMachine, "Move");
        reaperAttackState = new Enemy_ReaperAttackState(this, stateMachine, "Attack");
        reaperTeleportState = new Enemy_ReaperTeleportState(this, stateMachine, "Teleport");
        reaperSpellCastState = new Enemy_ReaperSpellCastState(this, stateMachine, "SpellCast");
        reaperPhaseShiftState = new Enemy_ReaperPhaseShiftState(this, stateMachine, "Idle");

        battleState = reaperBattleState;
        attackState = reaperAttackState;

        reaperSprite = anim != null ? anim.GetComponent<SpriteRenderer>() : GetComponentInChildren<SpriteRenderer>();
        if (reaperSprite != null)
            defaultSpriteColor = reaperSprite.color;
    }

    public void SetSpellCastPerformed(bool status)
    {
        spellCastPerformed = status;
    }
    public bool CanDoSpellCast()
    {
        return Time.time - lastTimeCastSpell > spellCooldown;
    }
    public void SetSpellOnCooldown() => lastTimeCastSpell = Time.time;

    public override void SpecialAttack()
    {
        StartCoroutine(CastSpellCo());
    }

    private IEnumerator CastSpellCo()
    {
        if (spellcastPrefab == null)
        {
            SetSpellCastPerformed(true);
            yield break;
        }

        if (player == null && TryFindTarget() == false)
        {
            SetSpellCastPerformed(true);
            yield break;
        }

        if(playerScript == null)
            playerScript = player.GetComponent<Player>();

        for (int i = 0; i < amountToCast; i++)
        {
            Vector3 spellPosition = GetPredictedSpellPosition();

            Enemy_ReaperSpell spell  = Instantiate(spellcastPrefab, spellPosition, Quaternion.identity).GetComponent<Enemy_ReaperSpell>();

            spell.SetupSpell(combat,spellDamageScale);

            yield return new WaitForSeconds(spellCastRate);
        }
        SetSpellCastPerformed(true);
    }

    public void SpawnSingleSpellAtPredictedPosition()
    {
        if (spellcastPrefab == null)
            return;

        Vector3 spellPosition = GetPredictedSpellPosition();
        Enemy_ReaperSpell spell = Instantiate(spellcastPrefab, spellPosition, Quaternion.identity).GetComponent<Enemy_ReaperSpell>();
        spell.SetupSpell(combat, spellDamageScale);
    }

    private Vector3 GetPredictedSpellPosition()
    {
        if (player == null && TryFindTarget() == false)
            return transform.position + new Vector3(0f, playerOffsetPrediction.y, 0f);

        if (playerScript == null && player != null)
            playerScript = player.GetComponent<Player>();

        Vector3 playerPosition = player.transform.position;
        Vector2 playerVelocity = playerScript != null ? playerScript.rb.velocity : Vector2.zero;

        float predictedX = playerPosition.x + playerVelocity.x * spellPredictionTime;
        if (Mathf.Abs(playerVelocity.x) > minPredictVelocity)
            predictedX += Mathf.Sign(playerVelocity.x) * playerOffsetPrediction.x;

        float clampedX = Mathf.Clamp(predictedX,
            playerPosition.x - maxHorizontalPrediction,
            playerPosition.x + maxHorizontalPrediction);

        return new Vector3(clampedX, playerPosition.y + playerOffsetPrediction.y, playerPosition.z);
    }

    public bool CanUseTeleport()
    {
        return phase2Triggered || disableTeleportInPhase1 == false;
    }

    public bool ShouldTeleport()
    {
        if (phaseShiftInProgress || CanUseTeleport() == false)
            return false;

        if(Random.value < chanceToTeleport)
        {
            chanceToTeleport = defaultTeleportChance;
            return true;
        }

        chanceToTeleport = Mathf.Clamp01(chanceToTeleport + 0.05f);
        return false;
    }

    protected override void Start()
    {
        base.Start();

        arenaBounds.transform.parent = null;
        defaultTeleportChance = chanceToTeleport;
        battleUnlocked = requireDamageToStartBattle == false;

        if (health != null)
        {
            health.OnTakingDamage -= UnlockBattleAggro;
            health.OnTakingDamage += UnlockBattleAggro;
            health.OnTakingDamage -= TryTriggerPhase2;
            health.OnTakingDamage += TryTriggerPhase2;
        }

        stateMachine.Initialize(idleState);
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnTakingDamage -= UnlockBattleAggro;
            health.OnTakingDamage -= TryTriggerPhase2;
        }

        if (stats != null)
            stats.offense.attackSpeed.RemoveModifier(Phase2AttackSpeedModifierSource);
    }

    public override RaycastHit2D IsPlayerDetected()
    {
        if (battleUnlocked == false)
            return default;

        return base.IsPlayerDetected();
    }

    protected override bool CanAcquireCombatTarget() => battleUnlocked;

    private void UnlockBattleAggro()
    {
        battleUnlocked = true;
    }

    public void SetBattleUnlocked(bool unlocked)
    {
        battleUnlocked = unlocked;

        if (unlocked == false)
            StopCombat();
    }

    public void BeginBossEncounter(Transform target = null)
    {
        if (health != null && health.isDead)
            return;

        battleUnlocked = true;

        if (target == null && Player.instance != null)
            target = Player.instance.transform;

        if (target != null)
            TryEnterBattleState(target);
    }

    private void TryTriggerPhase2()
    {
        if (phase2Triggered || health == null || health.isDead)
            return;

        if (health.getHealthPercent() > phase2TriggerHealthPercent)
            return;

        EnterPhase2();
    }

    private void EnterPhase2()
    {
        phase2Triggered = true;

        attackCooldown = Mathf.Max(.05f, attackCooldown * phase2AttackCooldownMultiplier);
        battleMoveSpeed = Mathf.Max(.1f, battleMoveSpeed * phase2BattleMoveSpeedMultiplier);
        spellCooldown = Mathf.Max(.1f, spellCooldown * phase2SpellCooldownMultiplier);
        spellCastRate = Mathf.Max(.05f, spellCastRate * phase2SpellCastRateMultiplier);
        amountToCast += Mathf.Max(0, phase2AdditionalSpellsPerCast);

        chanceToTeleport = Mathf.Clamp01(chanceToTeleport + phase2TeleportChanceBonus);
        defaultTeleportChance = chanceToTeleport;

        stats.offense.attackSpeed.RemoveModifier(Phase2AttackSpeedModifierSource);
        stats.offense.attackSpeed.AddModifier(phase2AttackSpeedBonus, Phase2AttackSpeedModifierSource);

        if (reaperSprite != null)
            reaperSprite.color = phase2TintColor;

        stateMachine.ChangeState(reaperPhaseShiftState);
    }

    public void OnPhaseShiftEnter()
    {
        phaseShiftInProgress = true;
        MakeUntargetable(true);
        health.SetCanTakeDamage(false);
    }

    public void OnPhaseShiftExit()
    {
        phaseShiftInProgress = false;
        vfx.StopImageEchoEffect();

        if (health != null && health.isDead == false)
            health.SetCanTakeDamage(true);

        if (health == null || health.isDead == false)
            MakeUntargetable(false);

        if (reaperSprite != null)
            reaperSprite.color = phase2Triggered ? phase2TintColor : defaultSpriteColor;
    }

    public int GetPhaseShiftTeleportCount() => Mathf.Max(1, phaseShiftTeleportCount);
    public float GetPhaseShiftTeleportInterval() => Mathf.Max(.05f, phaseShiftTeleportInterval);
    public float GetPhaseShiftEndDelay() => Mathf.Max(0f, phaseShiftEndDelay);
    public bool ShouldCastSpellInPhaseShift() => CanDoSpellCast();

    public IEnumerator PlayPhaseShiftSceneTransition()
    {
        UI_FadeScreen fadeScreen = ResolvePhaseShiftFadeScreen();
        if (fadeScreen == null)
        {
            ApplyPhaseShiftBackgroundColor();
            yield break;
        }

        fadeScreen.FadeOut(Mathf.Max(.01f, phaseShiftFadeOutDuration));
        if (fadeScreen.fadeEffectCo != null)
            yield return fadeScreen.fadeEffectCo;

        yield return new WaitForSeconds(Mathf.Max(0f, phaseShiftBlackScreenDuration));

        ApplyPhaseShiftBackgroundColor();

        fadeScreen.FadeIn(Mathf.Max(.01f, phaseShiftFadeInDuration));
        if (fadeScreen.fadeEffectCo != null)
            yield return fadeScreen.fadeEffectCo;
    }

    public void StartPhaseShiftEchoEffect()
    {
        float echoDuration = phaseShiftTeleportCount * phaseShiftTeleportInterval + phaseShiftEndDelay + .1f;
        vfx.DoImageEchoEffect(echoDuration);
    }

    public void CastPhaseShiftSpell()
    {
        if (CanDoSpellCast() == false)
            return;

        SpawnSingleSpellAtPredictedPosition();
        SetSpellOnCooldown();
    }

    public void PerformPhaseShiftStrike()
    {
        if (combat == null)
            return;

        if (player == null && TryFindTarget() == false)
            return;

        if (player == null)
            return;

        combat.PerformAttackOnTarget(player);
    }

    public Vector3 FindPointBehindCurrentTarget()
    {
        if (player == null && TryFindTarget() == false)
            return FindTeleportPoint();

        if (player == null)
            return FindTeleportPoint();

        if (playerScript == null)
            playerScript = player.GetComponent<Player>();

        int playerFacing = playerScript != null ? playerScript.facingDir : 1;
        float targetX = player.position.x - playerFacing * phaseShiftBehindOffset;

        return FindGroundedPointByX(targetX, player.position.y);
    }

    private Vector3 FindGroundedPointByX(float targetX, float fallbackY)
    {
        if (arenaBounds == null)
            return new Vector3(targetX, fallbackY + offsetCenterY, transform.position.z);

        float halfWidth = col != null ? col.bounds.extents.x : .5f;
        float minX = arenaBounds.bounds.min.x + halfWidth;
        float maxX = arenaBounds.bounds.max.x - halfWidth;
        float clampedX = Mathf.Clamp(targetX, minX, maxX);

        Vector2 raycastPoint = new Vector2(clampedX, arenaBounds.bounds.max.y);
        RaycastHit2D hit = Physics2D.Raycast(raycastPoint, Vector2.down, Mathf.Infinity, WhatIsGround);

        if (hit.collider != null)
            return hit.point + new Vector2(0f, offsetCenterY);

        return new Vector3(clampedX, fallbackY + offsetCenterY, transform.position.z);
    }

    public void FaceCurrentTarget()
    {
        int targetDirection = DirectionToCurrentTarget();
        if (targetDirection != 0)
            FlipController(targetDirection);
    }

    public void HandleCounter()
    {
        if (CanBeCountered == false)
            return;

        stateMachine.ChangeState(stunnedState);
    }

    public void StartBossBattleMusic()
    {
        if (AudioManager.instance == null)
            return;

        AudioManager.instance.StartBossBGM(bossMusicGroupName);
    }

    public void StopBossBattleMusic()
    {
        if (AudioManager.instance == null)
            return;

        AudioManager.instance.StopBossBGM();
    }

    public override void EntityDeath()
    {
        base.EntityDeath();
    }

    public void SetTeleportTrigger(bool triggerStatus) => teleportTrigger = triggerStatus;

    public Vector3 FindTeleportPoint()
    {
        int maxAttampts = 10;
        float bossWithColliderHalf = col.bounds.size.x / 2;

        for(int i = 0; i < maxAttampts; i++)
        {
            float randomX = Random.Range(arenaBounds.bounds.min.x + bossWithColliderHalf, arenaBounds.bounds.max.x - bossWithColliderHalf);

            Vector2 raycastPoint= new Vector2(randomX, arenaBounds.bounds.max.y);

            RaycastHit2D hit = Physics2D.Raycast(raycastPoint, Vector2.down, Mathf.Infinity,WhatIsGround);

            if(hit.collider != null)
                return hit.point + new Vector2(0, offsetCenterY);
        }

        return transform.position;
    }

    private UI_FadeScreen ResolvePhaseShiftFadeScreen()
    {
        if (phaseShiftFadeScreen != null)
            return phaseShiftFadeScreen;

        if (UI.instance != null && UI.instance.fadeScreenUI != null)
        {
            phaseShiftFadeScreen = UI.instance.fadeScreenUI;
            return phaseShiftFadeScreen;
        }

        UI_FadeScreen[] fadeScreens = FindObjectsByType<UI_FadeScreen>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (fadeScreens.Length > 0)
            phaseShiftFadeScreen = fadeScreens[0];

        return phaseShiftFadeScreen;
    }

    private void ApplyPhaseShiftBackgroundColor()
    {
        if (phaseShiftBackgroundApplied)
            return;

        CachePhaseShiftBackgroundRenderers();

        bool backgroundColorApplied = false;

        if (phaseShiftBackgroundRenderers != null)
        {
            foreach (SpriteRenderer backgroundRenderer in phaseShiftBackgroundRenderers)
            {
                if (backgroundRenderer == null)
                    continue;

                backgroundRenderer.color = phaseShiftBackgroundColor;
                backgroundColorApplied = true;
            }
        }

        if (phaseShiftBackgroundTilemaps != null)
        {
            foreach (Tilemap backgroundTilemap in phaseShiftBackgroundTilemaps)
            {
                if (backgroundTilemap == null)
                    continue;

                backgroundTilemap.color = phaseShiftBackgroundColor;
                backgroundColorApplied = true;
            }
        }

        phaseShiftBackgroundApplied = backgroundColorApplied;
    }

    private void CachePhaseShiftBackgroundRenderers()
    {
        bool hasCachedSpriteRenderers = phaseShiftBackgroundRenderers != null && phaseShiftBackgroundRenderers.Length > 0;
        bool hasCachedTilemaps = phaseShiftBackgroundTilemaps != null && phaseShiftBackgroundTilemaps.Length > 0;

        if (hasCachedSpriteRenderers || hasCachedTilemaps)
            return;

        HashSet<string> backgroundRootNames = GetPhaseShiftBackgroundRootNames();
        List<SpriteRenderer> backgroundRenderers = new List<SpriteRenderer>();
        List<Tilemap> backgroundTilemaps = new List<Tilemap>();
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject rootObject in rootObjects)
        {
            Transform[] sceneTransforms = rootObject.GetComponentsInChildren<Transform>(true);
            foreach (Transform sceneTransform in sceneTransforms)
            {
                if (backgroundRootNames.Contains(sceneTransform.name) == false)
                    continue;

                backgroundRenderers.AddRange(sceneTransform.GetComponentsInChildren<SpriteRenderer>(true));
                backgroundTilemaps.AddRange(sceneTransform.GetComponentsInChildren<Tilemap>(true));
            }
        }

        phaseShiftBackgroundRenderers = backgroundRenderers.ToArray();
        phaseShiftBackgroundTilemaps = backgroundTilemaps.ToArray();
    }

    private HashSet<string> GetPhaseShiftBackgroundRootNames()
    {
        HashSet<string> backgroundRootNames = new HashSet<string>();
        string configuredRootNames = string.IsNullOrWhiteSpace(phaseShiftBackgroundRootName) ? "BG,BackGround" : phaseShiftBackgroundRootName;
        string[] splitRootNames = configuredRootNames.Split(',');

        foreach (string splitRootName in splitRootNames)
        {
            string trimmedRootName = splitRootName.Trim();
            if (string.IsNullOrEmpty(trimmedRootName))
                continue;

            backgroundRootNames.Add(trimmedRootName);
        }

        backgroundRootNames.Add("BG");
        backgroundRootNames.Add("BackGround");
        return backgroundRootNames;
    }
}
