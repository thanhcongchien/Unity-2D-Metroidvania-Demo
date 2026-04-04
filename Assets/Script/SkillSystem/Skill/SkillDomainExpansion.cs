using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillDomainExpansion : SkillBase
{
    [SerializeField] private GameObject domainPrefab;

    [Header("Slowing Down Upgrade")]
    [SerializeField] private float slowdownPercent = 0.8f;
    [SerializeField] private float slowDownDomainInduration = 5f;

    [Header("Spell Cast Upgrade")]
    [SerializeField] private int spellsToCast = 10;
    [SerializeField] private float spellCastSlowdown = 1;
    [SerializeField] private float spellCastDuration = 8f;
    private float spellCastTimer;
    private float spellsCastPerSecond;

    [Header("Domain Details")]
    public float maxDomainSize = 10;
    public float expandSpeed = 3;

    private List<Enemy> TrappedTargets = new List<Enemy>();
    private Transform currentTarget;
    public void CreateDomain()
    {
        spellsCastPerSecond = spellsToCast / getDuration();

        GameObject domain = Instantiate(domainPrefab, transform.position, Quaternion.identity);
        domain.GetComponent<SkillObjectDomainExpansion>().SetUpDomain(this);

    }
    public void DoSpellCasting()
    {
        spellCastTimer -= Time.deltaTime;
        if (currentTarget == null)
            currentTarget = FindTargetInDomain();

        if(currentTarget != null&&spellCastTimer<0)
        {
            CastSpell(currentTarget);
            spellCastTimer = 1 / spellsCastPerSecond;
            currentTarget = null;   

        }

    }
    private void CastSpell(Transform target)
    {
        if(upgradeType==SkillUpgradeType.Domain_EchoSpam)
        {
            Vector3 offset = Random.value < .5f ? new Vector2(1, 0) : new Vector2(-1, 0);

            skillManager.timeEcho.CreateEcho(target.position + offset);
        }
        if(upgradeType==SkillUpgradeType.Domain_ShardSpam)
        {
            skillManager.shard.CreateRawShard(target,true);
        }
    }

    private Transform FindTargetInDomain()
    {
        if (TrappedTargets.Count == 0)
            return null;

        int randomIndex = Random.Range(0, TrappedTargets.Count);

        if (TrappedTargets[randomIndex].health.isDead)
        {
            TrappedTargets.RemoveAt(randomIndex);
            return null;

        }
        Transform target= TrappedTargets[randomIndex].transform;

        if (target == null)
        {
            TrappedTargets.RemoveAt(randomIndex);
            return null;

        }


        return target;


    }



    public float getDuration()
    {
        if (upgradeType == SkillUpgradeType.Domain_SlowingDown)
            return slowDownDomainInduration;
        else
            return spellCastDuration;
          
    }
    public float getSlowdownPercent()
    {
        if (upgradeType == SkillUpgradeType.Domain_SlowingDown)
            return slowdownPercent;
        else
            return spellCastSlowdown;
    }

    public bool InstantDomain()
    {
        return upgradeType != SkillUpgradeType.Domain_EchoSpam
            && upgradeType != SkillUpgradeType.Domain_ShardSpam;

    }



   
    public void AddTarget(Enemy targetToAdd)
    {
        TrappedTargets.Add(targetToAdd);
    }
    public void ClearTargets()
    {
        foreach(var enemy in TrappedTargets)
        {
            enemy.StopSlowdown();
        }


        TrappedTargets = new List<Enemy>();
    }
}
