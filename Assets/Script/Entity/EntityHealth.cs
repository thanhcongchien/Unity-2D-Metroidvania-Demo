using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class EntityHealth : MonoBehaviour,IDamagable
{
    public event Action OnTakingDamage;
    public event Action OnHealthUpdate;

    private Slider healthBar;
    private EntityStats entityStats;
    private Entity_DropManager dropManager;
   
     public bool isDead;
    [SerializeField] protected float currentHp;
    private EntityVFX vfx;
    private Entity entity;
    [Header("KnockBack")]
    [SerializeField] private Vector2 knockbackPower = new Vector2(1.5f, 2.5f);
    [SerializeField] private Vector2 HeavyKnockPower = new Vector2(7f, 7f);
    [SerializeField] private float knockbackDuration = .2f;
    [SerializeField] private float heaveKnockDuration = .5f;
    [Header("HeavyDamage")]
    [SerializeField] private float heavyDamageThreshold = .3f;
    [Header("Health Regen")]
    [SerializeField] private float regenInterval = 1f;
    [SerializeField] private bool CanRegen = true;
    public float lastDamageTaken;

    protected bool canTakeDamage = true;
    private bool miniHealthbarActive;

    protected virtual void Awake()
    {

        vfx = GetComponent<EntityVFX>();
        entity = GetComponent<Entity>();
        healthBar = GetComponentInChildren<Slider>();
        if (healthBar != null)
        {
            // 血条仅用于显示，禁用交互避免左右移动输入直接改写 Slider.value。
            healthBar.interactable = false;
            Navigation navigation = healthBar.navigation;
            navigation.mode = Navigation.Mode.None;
            healthBar.navigation = navigation;
        }
        entityStats = GetComponent<EntityStats>();
        dropManager = GetComponent<Entity_DropManager>();
        if (entityStats != null)
        {
            currentHp = entityStats.getMaxHealth();
            OnHealthUpdate += updateHealthBar;
            updateHealthBar();

            InvokeRepeating(nameof(RegenerateHp), 0, regenInterval);
        }
    }

    protected virtual void Start()
    { 
    }

    private void updateHealthBar()
    {
        if ((healthBar == null || entityStats == null)&&healthBar.transform.parent.gameObject.activeSelf==false)
        {
            return;
        }

        float maxHp = entityStats.getMaxHealth();
        if (maxHp <= 0f)
        {
            return;
        }

        float normalizedHealth = Mathf.Clamp01(currentHp / maxHp);
        healthBar.SetValueWithoutNotify(normalizedHealth);

    }

    public void EnableHealthbar(bool enable) => healthBar?.transform.parent.gameObject.SetActive(enable);

    public float getCurrentHp()=> currentHp;

    public float getHealthPercent()
    {
        if (entityStats == null)
            return 0f;

        float maxHp = entityStats.getMaxHealth();
        if (maxHp <= 0f)
            return 0f;

        return Mathf.Clamp01(currentHp / maxHp);
    }

    public void SetHealthToPercent(float percent)
    {
        currentHp = Mathf.Clamp(percent,0,1) * entityStats.getMaxHealth();
        OnHealthUpdate?.Invoke();
    }

    public void ForceDie()
    {
        if (isDead)
            return;

        Die();
    }
    

    public virtual bool takeDamage(float damage,float elementalDamage,ElementType element, Transform damageDealer)
    {
        if (isDead||!canTakeDamage) return false;

        if (AttackEvaded())
        {
            return false;
        }

        EntityStats attackerStats = damageDealer.GetComponent<EntityStats>();
        float armorReduction = attackerStats != null ? attackerStats.getArmorReduction() : 0;

        float mitigation = entityStats != null ? entityStats.getArmorMitigation(armorReduction) : 0;
        float finalPhysicalDamage = damage * (1 - mitigation);

        float resistance = entityStats != null ? entityStats.getElementalResistance(element) : 0;
        float finalElementalDamage = elementalDamage * (1 - resistance);

        TakeKnockBack(damageDealer, finalPhysicalDamage);

        ReduceHp(finalPhysicalDamage+finalElementalDamage);

        lastDamageTaken = finalElementalDamage + finalPhysicalDamage;

        OnTakingDamage?.Invoke();

        return true;
    }

   public void SetCanTakeDamage(bool canTakeDamage)=> this.canTakeDamage = canTakeDamage;

    private bool AttackEvaded()
    {
        if(entityStats==null)
            return false;
        else
            return UnityEngine.Random.Range(0, 100) < entityStats.getEvasion(); 
    }

    private void RegenerateHp()
    {

        if(CanRegen==false)
            return;

        float regenAmount = entityStats.resources.healthRegen.GetValue();
        IncreaseHp(regenAmount);
    }



    public void IncreaseHp(float healAmount)
    {

        if(isDead)
            return;

        float newHp=currentHp+healAmount;
        float maxHp = entityStats.getMaxHealth();

        currentHp=Mathf.Min(newHp, maxHp);
        OnHealthUpdate?.Invoke();
        
    }    








     public void ReduceHp(float damage)
    {
        vfx.PlayOnDamageVfx();
        currentHp -=damage;

       OnHealthUpdate?.Invoke();
        if (currentHp<=0)
        {
            Die();
        }

    }
    
    protected virtual  void Die()
    {
        isDead = true;
        entity.EntityDeath();
        dropManager?.DropItems();
    }

    private void TakeKnockBack(Transform damageDealer, float finalDamage)
    {
        Vector2 knockback = CalculateKnockback(finalDamage, damageDealer);
        float duration = CalculateDuration(finalDamage);
        entity.ReceiveKnockback(knockback, duration);
    }

    private Vector2 CalculateKnockback(float damage, Transform damageDealer)
    {
        int direction = transform.position.x > damageDealer.position.x ? 1 : -1;

        Vector2 knockback = isHeavyDamage(damage) ? HeavyKnockPower : knockbackPower;
        knockback.x=knockback.x*direction;
        return knockback;
    }
    private float CalculateDuration(float damage) => isHeavyDamage(damage) ? heaveKnockDuration : knockbackDuration;
    private bool isHeavyDamage(float damage)
    {
        if(entityStats==null) return false;

        else
            return damage / entityStats.getMaxHealth() > heavyDamageThreshold;
    }

}

