using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDashState : PlayerState
{
    
    public PlayerDashState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (player.sfx != null)
            player.sfx.PlayDash();

        if (player.CanQuickDash())
        {
            skillManager.dash.OnStartEffect();
            player.playerVFX.DoImageEchoEffect(player.dashDuration);
            player.gameObject.layer= LayerMask.NameToLayer("Untargetable");
        }
        stateTimer = player.dashDuration;
        player.health.SetCanTakeDamage(false);

    }

    public override void Exit()
    {
        base.Exit();
        player.health.SetCanTakeDamage(true);
        skillManager.dash.OnEndEffect();
        player.SetVelocity(0, rb.velocity.y);
        player.gameObject.layer = LayerMask.NameToLayer("Player");
    }

    public override void Update()
    {
        base.Update();
        if (!player.isGroundDetected()&&player.isWallDetected())
        {
            stateMachine.ChangeState(player.wallslideState);
        }
        float currentDashSpeed = player.CanQuickDash() ? player.QuickDashSpeed : player.dashSpeed;
        player.SetVelocity(currentDashSpeed * player.dashDir, 0);
       
        if (stateTimer < 0)
            stateMachine.ChangeState(player.idleState);
    }

    private void CancelDashIfNeeded()
    {
        if (player.isWallDetected())
        {
            if (player.isGroundDetected())
                stateMachine.ChangeState(player.idleState);
            else
                stateMachine.ChangeState(player.wallslideState);
        }
    }
}
