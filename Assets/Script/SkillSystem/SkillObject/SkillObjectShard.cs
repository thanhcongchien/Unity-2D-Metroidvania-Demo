using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillObjectShard : SkillObjectBase
{
    public event Action OnExplode;

    private SkillShard skillShard;

   [SerializeField] private GameObject VfxPre;

    private Transform target;
    private float speed;


    private void Update()
    {
        if (target == null)
            return;

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
    }
    public void MoveToCloestTarget(float speed,Transform newTarget = null)
    {
        target = newTarget == null ? FindClosetTarget() : newTarget;
        this.speed = speed;

    }

    public void SetUpShard(SkillShard skillShard)
    {
        this.skillShard = skillShard;

        playeStats=skillShard.player.stats;
        damageScaleData = skillShard.damageScaleData; 

        float detonationTime = skillShard.GetDetonateTime();

        Invoke(nameof(Explode), detonationTime);

    }

    public void SetUpShard(SkillShard skillShard,float detonationTime,bool canMove,float shardSpeed,Transform target)
    {
        this.skillShard = skillShard;

        playeStats = skillShard.player.stats;
        damageScaleData = skillShard.damageScaleData;

        

        Invoke(nameof(Explode), detonationTime);

        if(canMove)
        {
            MoveToCloestTarget(shardSpeed,target);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Enemy>() == null)
            return;

        Explode();
    }
   


    public void Explode()
    {
        DamageEnemiesInRadius(transform,checkRadius);
        GameObject vfx = Instantiate(VfxPre, transform.position, Quaternion.identity);
        vfx.GetComponentInChildren<SpriteRenderer>().color = skillShard.player.playerVFX.getElementColor(usedElement);
        OnExplode?.Invoke();
        Destroy(gameObject);
    }
}
