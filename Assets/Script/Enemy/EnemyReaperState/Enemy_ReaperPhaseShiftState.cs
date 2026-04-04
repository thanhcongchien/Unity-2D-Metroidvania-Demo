using System.Collections;
using UnityEngine;

public class Enemy_ReaperPhaseShiftState : EnemyState
{
    private Enemy_Reaper enemyReaper;
    private Coroutine phaseShiftCo;
    private bool sequenceFinished;

    public Enemy_ReaperPhaseShiftState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        enemyReaper = enemy as Enemy_Reaper;
    }

    public override void Enter()
    {
        base.Enter();

        sequenceFinished = false;
        enemyReaper.SetVelocity(0, 0);

        phaseShiftCo = enemyReaper.StartCoroutine(PhaseShiftCo());
    }

    public override void Update()
    {
        base.Update();
        enemyReaper.ZeroVelocity();

        if (sequenceFinished)
            stateMachine.ChangeState(enemyReaper.reaperBattleState);
    }

    public override void Exit()
    {
        if (phaseShiftCo != null)
            enemyReaper.StopCoroutine(phaseShiftCo);

        enemyReaper.OnPhaseShiftExit();
        base.Exit();
    }

    private IEnumerator PhaseShiftCo()
    {
        enemyReaper.OnPhaseShiftEnter();
        yield return enemyReaper.PlayPhaseShiftSceneTransition();
        enemyReaper.StartPhaseShiftEchoEffect();

        int teleportCount = enemyReaper.GetPhaseShiftTeleportCount();
        float teleportInterval = enemyReaper.GetPhaseShiftTeleportInterval();

        for (int i = 0; i < teleportCount; i++)
        {
            if (enemy.HasValidTarget() == false)
                enemy.TryFindTarget();

            enemyReaper.transform.position = enemyReaper.FindPointBehindCurrentTarget();
            enemyReaper.FaceCurrentTarget();
            enemyReaper.PerformPhaseShiftStrike();

            if (enemyReaper.ShouldCastSpellInPhaseShift())
                enemyReaper.CastPhaseShiftSpell();

            yield return new WaitForSeconds(teleportInterval);
        }

        yield return new WaitForSeconds(enemyReaper.GetPhaseShiftEndDelay());
        sequenceFinished = true;
    }
}
