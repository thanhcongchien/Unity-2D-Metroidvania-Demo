using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class SkillObjectDomainExpansion : SkillObjectBase
{
    private SkillDomainExpansion domainManager;

    
    private float expandspeed = 2;
    private float duration;
    private float slowdownpercent = .9f;

    private Vector3 targetScale;
    private bool isShrinking;

    public void SetUpDomain(SkillDomainExpansion domainManager)
    {
        this.domainManager = domainManager;

        duration = domainManager.getDuration();
        float maxSize = domainManager.maxDomainSize;
        slowdownpercent = domainManager.getSlowdownPercent();
        expandspeed = domainManager.expandSpeed;

        targetScale = Vector3.one * maxSize;
        Invoke(nameof(ShirinkDomain), duration);
    }
    private void Update()
    {
        HandleScaling();
    }


    private void HandleScaling()
    {
        float sizeDifference = Mathf.Abs(transform.localScale.x - targetScale.x);
        bool shouldChangeScale = sizeDifference > .1f;
        if(shouldChangeScale)
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, expandspeed * Time.deltaTime);

        if(isShrinking&& sizeDifference < .1f)
        { 
            domainManager.ClearTargets();
            Destroy(gameObject);
        }

    }

    private void ShirinkDomain()
    {
        targetScale = Vector3.zero;
        isShrinking = true;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy == null)
            return;

        domainManager.AddTarget(enemy);
        enemy.SlowDownEntity(duration, slowdownpercent,true);

    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy == null)
            return;

        enemy.StopSlowdown();

    }

}
