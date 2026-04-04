using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMage : Enemy,ICounterable
{
    public bool CanBeCountered { get => canBeStunned; }
    public Enemy_MageRetreatState retreatState { get; private set;}
    public Enemy_MageBattleState mageBattleState { get; private set;}

    public Enemy_SpellCastState spellCastState { get; private set;}

    [Header("Mage specifics")]
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private int amounToCast = 3;
    [SerializeField] private float spellCastCooldown = .3f;
    [SerializeField] private Transform spellStartPos;
    [Header("Elemental Spells")]
    [SerializeField] private DamageScaleData spellDamageScale = new DamageScaleData
    {
        physical = .35f,
        elemental = 1f,
        chillDuration = 2.5f,
        chillSlowMultiplier = .3f,
        burnDuration = 3f,
        burnDamageScale = .65f,
        lightningDuration = 3f,
        lightningDamageScale = .65f,
        lightningCharge = .6f
    };
    [SerializeField] private float fireSpellDamage = 24f;
    [SerializeField] private float iceSpellDamage = 20f;
    [SerializeField] private float lightningSpellDamage = 18f;
    public bool spellCastPerformed { get; private set;}
  
    [SerializeField] private bool hasRecoveryAnimation = true;
    [Space]
    public float retreatCooldown = 5f;
    public float retreatMaxDistance = 8f;
    public float retreatSpeed = 15f;
    protected override void Awake()
    {
        base.Awake();

        idleState = new EnemyIdleState(this, stateMachine, "Idle");
        moveState = new EnemyMoveState(this, stateMachine, "Move");
       
        attackState = new EnemyAttackState(this, stateMachine, "Attack");
        stunnedState = new EnemyStunnedState(this, stateMachine, "Stun");
        deadState = new EnemyDeadState(this, stateMachine, "Idle"); ;

        retreatState = new Enemy_MageRetreatState(this, stateMachine, "Move");
        mageBattleState = new Enemy_MageBattleState(this, stateMachine, "Move");
        spellCastState = new Enemy_SpellCastState(this, stateMachine, "SpellCast");
        battleState = mageBattleState;

        anim.SetBool("hasStunRecovery", hasRecoveryAnimation); 
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }
    public void SetSpellCastPerformed(bool performed) => spellCastPerformed = performed;

    public override void SpecialAttack()
    {
        StartCoroutine(CastSpellCo());
    }

    private IEnumerator CastSpellCo()
    {
        if (spellPrefab == null)
        {
            SetSpellCastPerformed(true);
            yield break;
        }

        if (player == null && TryFindTarget() == false)
        {
            SetSpellCastPerformed(true);
            yield break;
        }

        ElementType spellElement = GetRandomSpellElement();
        float elementalDamage = GetSpellDamageByElement(spellElement) * spellDamageScale.elemental;
        ElementalEffectData effectData = new ElementalEffectData(spellElement, elementalDamage, spellDamageScale);

        for (int i = 0; i < amounToCast; i++)
        {
            Transform currentTarget = GetDetectedTarget();
            if (currentTarget == null)
                break;

            float physicalDamage = stats.GetBaseDamage() * spellDamageScale.physical;

            Enemy_MageProjectile projectile = Instantiate(spellPrefab, spellStartPos.position, Quaternion.identity).GetComponent<Enemy_MageProjectile>();
            projectile.SetupProjectile(currentTarget, combat, physicalDamage, elementalDamage, spellElement, effectData);
           
            yield return new WaitForSeconds(spellCastCooldown);
        }
        SetSpellCastPerformed(true);
    }

    private ElementType GetRandomSpellElement()
    {
        int randomElementIndex = Random.Range(1, 4);
        return (ElementType)randomElementIndex;
    }

    private float GetSpellDamageByElement(ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire:
                return fireSpellDamage;
            case ElementType.Ice:
                return iceSpellDamage;
            case ElementType.Lightning:
                return lightningSpellDamage;
            default:
                return 0f;
        }
    }

    public void HandleCounter()
    {
        if (CanBeCountered == false)
            return;

        stateMachine.ChangeState(stunnedState);
    }
}
