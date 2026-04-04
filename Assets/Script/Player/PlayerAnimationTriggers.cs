using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationTriggers : EntityAnimationTrigger
{
    private Player player;
    protected override void Awake()
    {
        base.Awake();
     player=GetComponentInParent<Player>();   
    }

    private void ThrowSword()=>player.skillManager.swordThrow.ThrowSword();

}
