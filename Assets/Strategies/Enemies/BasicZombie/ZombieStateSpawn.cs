using UnityEngine;

public class ZombieStateSpawn : StateMachineState
{
    public ZombieStateSpawn(Zombie enemy, string animationName, AudioClip[] clips, StateMachine stateMachine) : base(enemy, animationName, clips, stateMachine)
    {
        limitTime = 1;
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        if (elapsedTime >= limitTime) stateMachine.ChangeState(enemy.IdleState);
    }

}