using Unity.VisualScripting;
using UnityEngine;

public class SecondBossStateDash : StateMachineState
{
    private new readonly BossZombie enemy;
    public SecondBossStateDash(BossZombie enemy, string animationName, AudioClip[] clips, StateMachine stateMachine) : base(enemy, animationName, clips, stateMachine)
    {
        this.enemy = enemy;
        limitTime = enemy.DashLength;
        oneshotAnimation=true;
        oneshotAudioclip=true;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.StartDash(false);
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        if (elapsedTime >= limitTime)
            if (enemy.IsTargetInChaseRange()) 
                stateMachine.ChangeState(enemy.ChaseState);
            else
                stateMachine.ChangeState(enemy.IdleState);
    }

}