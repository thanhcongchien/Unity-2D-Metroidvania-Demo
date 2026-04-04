using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwordThrowState : PlayerState
{
    private Camera mainCamera;
    private bool throwStarted;

    public PlayerSwordThrowState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        throwStarted = false;
        skillManager.swordThrow.EnableDots(true);

        if(mainCamera!=Camera.main)
        {
            mainCamera = Camera.main;
        }

        skillManager.swordThrow.PredictTrajectory(DirectionToMouse());
    }
    public override void Update()
    {
        base.Update();

        // 投剑期间实时读取鼠标方向（鼠标位置来自 Player 输入快照）。
        Vector2 dirToMouse=DirectionToMouse();

        player.SetVelocity(0, rb.velocity.y);
        player.FlipController(dirToMouse.x);

        if (!throwStarted)
        {
            skillManager.swordThrow.PredictTrajectory(dirToMouse);

            // 左键确认投掷。
            if (player.input.primaryAttackPressed)
            {
                throwStarted = true;
                skillManager.swordThrow.ConfirmTrajectory(dirToMouse);
                skillManager.swordThrow.EnableDots(false);
                anim.SetBool("SwordThrowPerformed", true);
            }
        }

        // 右键抬起取消蓄力并回到待机。
        if (!throwStarted && player.input.secondaryAttackReleased)
            stateMachine.ChangeState(player.idleState);

        if (throwStarted && triggerCalled)
            stateMachine.ChangeState(player.idleState);
    }
    public override void Exit()
    {
        base.Exit();

        throwStarted = false;
        anim.SetBool("SwordThrowPerformed", false);
        skillManager.swordThrow.EnableDots(false);
    }

    private Vector2 DirectionToMouse()
    {
        Vector2 playerPos=player.transform.position;
        // 鼠标屏幕坐标统一由 Player 在本帧采集并缓存。
        Vector3 mouseScreenPos = player.mousePosition;
        mouseScreenPos.z = Mathf.Abs(mainCamera.transform.position.z - player.transform.position.z);
        Vector2 worldMousePos=mainCamera.ScreenToWorldPoint(mouseScreenPos);

        Vector2 direction=worldMousePos-playerPos;

        return direction.normalized;
    }
}

