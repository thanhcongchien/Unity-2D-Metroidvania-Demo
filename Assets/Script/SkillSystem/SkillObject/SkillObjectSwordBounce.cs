using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillObjectSwordBounce : SkillObjectSword
{
    [SerializeField] private float bounceSpeed = 15;
    private int bounceCount;

    private Collider2D[] enemyTargets;
    private Transform nextTarget;
    private List<Transform> selectedBefore = new List<Transform>();


    public override void SetUpSword(SkillSwordThrow swordManager, Vector2 dir)
    {
        anim?.SetTrigger("Spin");
        base.SetUpSword(swordManager, dir);

        bounceCount = swordManager.bounceCount;
        bounceSpeed = swordManager.bounceSpeed;
    }

    protected override void Update()
    {
        HandleComeback();
        HandleBounce();
    }
    private void HandleBounce()
    {
        if(nextTarget==null)
            return;

        transform.position = Vector2.MoveTowards(transform.position, nextTarget.position, Time.deltaTime * bounceSpeed);
        if(Vector2.Distance(transform.position, nextTarget.position)<.75f)
        {
            DamageEnemiesInRadius(transform, 1);
            BounceToNext();

            if(bounceCount==0||nextTarget==null)
            {
                nextTarget = null;
                GetSwordBack();
            }
        }
    }

    private void BounceToNext()
    {
        nextTarget = GetNextTarget();
        bounceCount--;
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (enemyTargets == null)
        {
            enemyTargets = getEnemiesAround(transform, 10);
            rb.simulated = false;
        }
        DamageEnemiesInRadius(transform, 1);

        if (enemyTargets.Length <= 1 || bounceCount == 0)
            GetSwordBack();
        else
            nextTarget = GetNextTarget();

    }
    private Transform GetNextTarget()
    {
        List<Transform> validTargets = getValidTargets();

        int randomIndex = Random.Range(0, validTargets.Count);

        Transform nextTarget = validTargets[randomIndex];
        selectedBefore.Add(nextTarget);

        return nextTarget;
    }

    private List<Transform> getValidTargets()
    {
        List<Transform> validTargets = new List<Transform>();
        List<Transform> aliveTargets = getAliveTargets();

        foreach(var enemy in aliveTargets)
        {
          if(enemy != null&&selectedBefore.Contains(enemy.transform)==false)
            {
                validTargets.Add(enemy.transform);
            }
        }

        if (validTargets.Count > 0)
            return validTargets;
        else
        {
            selectedBefore.Clear();
            return aliveTargets;
        }

    }


    private List<Transform> getAliveTargets()
    {
        List<Transform> aliveTargets = new List<Transform>();

        foreach(var enemy in enemyTargets)
        {
            if(enemy!=null)
                aliveTargets.Add(enemy.transform);
        }
        return aliveTargets;
    }



   
}
