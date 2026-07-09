using UnityEngine;

public class ZombieStateDie : StateMachineState
{
    public ZombieStateDie(Zombie enemy, string animationName, AudioClip[] clips, StateMachine stateMachine) : base(enemy, animationName, clips, stateMachine)
    {
        limitTime = 1;
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        enemy.Die();
    }

}