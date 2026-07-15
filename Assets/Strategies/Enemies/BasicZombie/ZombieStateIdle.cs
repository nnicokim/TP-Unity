using UnityEngine;

public class ZombieStateIdle : StateMachineState
{

    public ZombieStateIdle(Zombie enemy, string animationName, AudioClip[] clips, StateMachine stateMachine) : base(enemy, animationName, clips, stateMachine)
    {
        limitTime = -1;
        oneshotAnimation=false;
        oneshotAudioclip=false;
        cooldownTime = 1;
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        if (enemy.IsTargetInChaseRange() && elapsedTime >= cooldownTime) stateMachine.ChangeState(enemy.ChaseState);
        else enemy.WalkAround();
    }

}