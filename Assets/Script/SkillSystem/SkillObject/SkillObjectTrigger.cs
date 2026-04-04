using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillObjectTrigger : MonoBehaviour
{
    private SkillObjectTimeEcho timeEcho;
    private int performedAttacks;

    private void Awake()
    {
        timeEcho = GetComponentInParent<SkillObjectTimeEcho>();
        performedAttacks = 0;
    }

    private void AttackTrigger()
    {
        performedAttacks++;
        timeEcho.PerformAttack();
    }

    private void AnimationTrigger()
    {
        TryTerminate(performedAttacks);
    }

    private void TryTerminate(int currentAttackIndex)
    {
        if(currentAttackIndex==timeEcho.maxAttacks)
        {
            timeEcho.HandleDeath();
        }
    }
}
