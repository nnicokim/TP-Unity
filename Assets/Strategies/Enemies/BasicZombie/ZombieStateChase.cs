using UnityEngine;

public class ZombieStateChase : StateMachineState
{

    public ZombieStateChase(Zombie enemy, string animationName, AudioClip[] clips, StateMachine stateMachine) : base(enemy, animationName, clips, stateMachine)
    {
        limitTime = -1;
        oneshotAnimation=false;
        oneshotAudioclip=false;
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        if (!enemy.IsTargetInChaseRange() && elapsedTime >= cooldownTime) 
            stateMachine.ChangeState(enemy.IdleState);
        else if (enemy.IsTargetInAttackRange() && elapsedTime >= cooldownTime)
            stateMachine.ChangeState(enemy.AttackState);
        else enemy.ChaseTarget();
    }

}