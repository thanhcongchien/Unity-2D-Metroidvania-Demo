using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class SkillShard :SkillBase
{
    private SkillObjectShard currentShard;
    private EntityHealth playerHealth;

    [SerializeField] private GameObject ShardPre;
    [SerializeField] private float detinationTime = 2f;
    [Header("Moving")]
    [SerializeField]private float ShardSpeed = 7f;
    [Header("Multicast")]
    [SerializeField] private int maxCharge = 3;
    [SerializeField] private int currentCharges;
    [SerializeField] private bool isCharging;
    [Header("Teleport")]
    [SerializeField] private float shardExistDuration = 10f;
    [Header("Health Rewind")]
    [SerializeField] private float savedHealthPercent;
    public override void TryUseSkill()
    {
        if (CanUseSkill()==false) return;

        if (Unlocked(SkillUpgradeType.Shard))
            HandleShardRegular();
        if(Unlocked(SkillUpgradeType.Shard_MoveToEnemy))
            HandleShardMoving();
        if(Unlocked(SkillUpgradeType.Shard_Multicast))
            HandleShardMulticast();
        if(Unlocked(SkillUpgradeType.Shard_Teleport))
            HandleShardTeleport();
        if(Unlocked(SkillUpgradeType.Shard_TeleportHpRewind))
            HandleShardHpRewind();
    }
    protected override void Awake()
    {
        base.Awake();
        currentCharges=maxCharge;
        playerHealth = GetComponentInParent<EntityHealth>();
    }
    public void CreateShard()
    {
        float detonationTime = GetDetonateTime();

        GameObject shard = Instantiate(ShardPre, transform.position, Quaternion.identity);

        currentShard = shard.GetComponent<SkillObjectShard>();
        currentShard.SetUpShard(this);

        if (Unlocked(SkillUpgradeType.Shard_Teleport) || Unlocked(SkillUpgradeType.Shard_TeleportHpRewind))
            currentShard.OnExplode += ForceCooldown;
    }
    public void CreateRawShard(Transform target=null,bool shardCanMove=false)
    {
        bool canMove = shardCanMove!=false ? shardCanMove:Unlocked(SkillUpgradeType.Shard_MoveToEnemy) || Unlocked(SkillUpgradeType.Shard_Multicast);


        GameObject shard = Instantiate(ShardPre, transform.position, Quaternion.identity);
        shard.GetComponent<SkillObjectShard>().SetUpShard(this, detinationTime, canMove, ShardSpeed,target);
    }
    public void CreateDomainShard(Transform target)
    {


    }
    private void HandleShardHpRewind()
    {
       
        if (currentShard == null)
        {
           
            CreateShard();
            savedHealthPercent=playerHealth.getHealthPercent();
        }
        else
        {
            SwapPlayerAndShard();
            playerHealth.SetHealthToPercent(savedHealthPercent);
            SetSkillCooldown();
        }
    }
    private void HandleShardTeleport()
    {
        if(currentShard == null)
            CreateShard();
        else
        {
            SwapPlayerAndShard();
            SetSkillCooldown();
        }

    }
    private void SwapPlayerAndShard()
    {
        Vector3 shardPos=currentShard.transform.position;
        Vector3 playerPos=player.transform.position;

        currentShard.transform.position=playerPos;
        currentShard.Explode();
        player.TeleportPlayer(shardPos);
    }
    private void HandleShardMulticast()
    {
        if(currentCharges<=0)
            return;

        CreateShard();
        currentShard.MoveToCloestTarget(ShardSpeed);
        currentCharges--;

        if(isCharging==false)
            StartCoroutine(ShardRechargeCo());
    }

    private IEnumerator ShardRechargeCo()
    {

        isCharging = true;

        while(currentCharges<maxCharge)
        {
            currentCharges++;
            yield return new WaitForSeconds(cooldown);
        }
        isCharging=false;
    }
    private void HandleShardMoving()
    {
        CreateShard();
        currentShard.MoveToCloestTarget(ShardSpeed);

        SetSkillCooldown();

    }
    private void HandleShardRegular()
    {
        CreateShard();
        SetSkillCooldown();
    }

   
    public float GetDetonateTime()
    {
        if(Unlocked(SkillUpgradeType.Shard_Teleport)||Unlocked(SkillUpgradeType.Shard_TeleportHpRewind))
        return shardExistDuration;

        return detinationTime;
    }
    private void ForceCooldown()
    {
        if (OnCooldown() == false)
        {
            SetSkillCooldown();
            currentShard.OnExplode-=ForceCooldown;

        }
    }
}
