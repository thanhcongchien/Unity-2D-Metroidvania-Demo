using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityStatusHandller : MonoBehaviour
{
    private const float AdditionalChillSlowMultiplier = 0.1f;

    private Entity entity;
    private EntityVFX entityVFX;
    private EntityStats stats;
    private EntityHealth health;
   private ElementType currrentEffect=ElementType.None;

    [Header("Electrify effct")]
     public GameObject electrifyStrikeVfx;
    [SerializeField] private float currentCharge;
    [SerializeField] private float maxCharge = 1;
    private Coroutine electrifyCo;

    private void Awake()
    {
        entity = GetComponent<Entity>();
        entityVFX = GetComponent<EntityVFX>();
        stats = GetComponent<EntityStats>();
        health = GetComponent<EntityHealth>();
    }

    public void RemoveAllNegativeEffects()
    {

        StopAllCoroutines();
        currrentEffect = ElementType.None;
        entityVFX.StopAllVFX();
    }

    public void ApplyStatusEffect(ElementType element,ElementalEffectData EffectData)
    {
        if(element==ElementType.Ice&&CanbeApplied(ElementType.Ice))
            ApplyChillEffect(EffectData.chillDuration,EffectData.chillSlowEffect);

        if(element==ElementType.Fire&&CanbeApplied(ElementType.Fire))
            ApplyBurnEffect(EffectData.burnDuration,EffectData.totalBurnDamage);

        if(element==ElementType.Lightning&&CanbeApplied(ElementType.Lightning))
            ApplyLightningEffect(EffectData.lightningDuration,EffectData.lightningDamage,EffectData.lightningCharge);

    }
    public void ApplyLightningEffect(float duration,float Damage,float charge)
    { 
        float lightningRes=stats.getElementalResistance(ElementType.Lightning);
        float finalCharge=charge*(1-lightningRes);

        currentCharge += finalCharge;
        if (currentCharge >= maxCharge)
        {
            Lightning(Damage);
            StopElectrifyEffect();
            return;
        }
        if (electrifyCo!=null)
            StopCoroutine(electrifyCo);

        electrifyCo = StartCoroutine(ElectrifyEffectCo(duration));
    }

    private void Lightning(float Damage)
    {
       
        Instantiate(electrifyStrikeVfx, transform.position, Quaternion.identity);
        health.ReduceHp(Damage);
    }

    private void StopElectrifyEffect()
    {
        currrentEffect=ElementType.None;
        currentCharge = 0;
        entityVFX.StopAllVFX();
    }





    public void ApplyBurnEffect(float duration,float fireDamage)
    {
        float fireRes=stats.getElementalResistance(ElementType.Fire);
        float finalDamage=fireDamage*(1-fireRes);

        StartCoroutine(BurnEffectCo(duration,finalDamage));
    }




    public void ApplyChillEffect(float duration, float slowMultiplier)
    { 
        float iceRes=stats.getElementalResistance(ElementType.Ice);
        float reduceDuration=duration*(1-iceRes);
        float finalSlowMultiplier = Mathf.Clamp01(slowMultiplier + AdditionalChillSlowMultiplier);

        StartCoroutine(ChilledEffectCo(reduceDuration, finalSlowMultiplier));

    }


    private IEnumerator ElectrifyEffectCo(float duration)
    {
        currrentEffect = ElementType.Lightning;
        entityVFX.PlayStatusVFX(ElementType.Lightning,duration);

        yield return new WaitForSeconds(duration);
        StopElectrifyEffect();

    }
    private IEnumerator BurnEffectCo(float duration,float totalDamage)
    {
        currrentEffect = ElementType.Fire;
        entityVFX.PlayStatusVFX(ElementType.Fire,totalDamage);

        int ticksPersecond = 2;
        int tickCount = Mathf.RoundToInt(duration * ticksPersecond);

        float damagePerTick = totalDamage / tickCount;
        float tickInterval = 1f / ticksPersecond;

        for(int i = 0; i < tickCount; i++)
        {
            health.ReduceHp(damagePerTick);
            yield return new WaitForSeconds(tickInterval);
        }
        currrentEffect = ElementType.None;
    }



    private IEnumerator ChilledEffectCo(float duration,float slowMultiplier)
    {
        entity.SlowDownEntity(duration, slowMultiplier);
        currrentEffect =ElementType.Ice;
        entityVFX.PlayStatusVFX(ElementType.Ice,duration);
        yield return new WaitForSeconds(duration);

        currrentEffect = ElementType.None;
    }



   public bool CanbeApplied(ElementType element)
   {
        if(element==ElementType.Lightning&&currrentEffect==ElementType.Lightning)
            return true;


        return currrentEffect == ElementType.None;
   }
}
