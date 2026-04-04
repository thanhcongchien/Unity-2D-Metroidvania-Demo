using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCounterAttackState : PlayerState
{
    private PlayerCombat combat;
    private bool CounteredSomeone;
    public PlayerCounterAttackState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
        combat=player.GetComponent<PlayerCombat>();

    }

    public override void Enter()
    {
        base.Enter();
        CounteredSomeone = combat.CounterAttackPeformed();
        stateTimer = player.CounterAttackDuration;
        player.anim.SetBool("SuccessfulCounterAttack", false);

        if (CounteredSomeone && player.sfx != null)
            player.sfx.PlaySuccessfulCounterAttack();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        player.ZeroVelocity();

       if(CounteredSomeone)
        {
            anim.SetBool("SuccessfulCounterAttack", true);
           
        }

       if(triggerCalled)
            stateMachine.ChangeState(player.idleState);

        if (stateTimer < 0&&CounteredSomeone==false )
            stateMachine.ChangeState(player.idleState);

    }
}
