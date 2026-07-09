using UnityEngine;

public class ZombieStateHurt : StateMachineState
{
    public ZombieStateHurt(Zombie enemy, string animationName, AudioClip[] clips, StateMachine stateMachine) : base(enemy, animationName, clips, stateMachine)
    {
        limitTime = 1;
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        if (elapsedTime >= limitTime)
        {
            if (enemy.IsTargetInChaseRange())
                stateMachine.ChangeState(enemy.ChaseState);
            else 
                stateMachine.ChangeState(enemy.IdleState);
        }
    }

}