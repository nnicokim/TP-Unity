using UnityEngine;

public class ZombieStateIdle : StateMachineState
{

    public ZombieStateIdle(Zombie enemy, string animationName, AudioClip[] clips, StateMachine stateMachine) : base(enemy, animationName, clips, stateMachine)
    {
        limitTime = -1;
        oneshotAnimation=false;
        oneshotAudioclip=false;
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        if (enemy.IsTargetInChaseRange()) stateMachine.ChangeState(enemy.ChaseState);
        else enemy.WalkAround();
    }

}