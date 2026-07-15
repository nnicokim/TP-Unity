using UnityEngine;

public class ZombieStateAttack : StateMachineState
{
    private readonly float _PreattackTime = 0.5f;
    private bool _HasAttacked = false;
    public ZombieStateAttack(Zombie enemy, string animationName, AudioClip[] clips, StateMachine stateMachine) : base(enemy, animationName, clips, stateMachine)
    {
        limitTime = 1;
    }

    public override void Enter()
    {
        base.Enter();
        _HasAttacked = false;
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        if (elapsedTime >= _PreattackTime && !_HasAttacked)
        {
            _HasAttacked = true;
            enemy.AttackTargetInRange();
        }
        if (elapsedTime >= limitTime)
        {
            if (enemy.IsTargetInChaseRange())
                stateMachine.ChangeState(enemy.ChaseState);
            else 
                stateMachine.ChangeState(enemy.IdleState);
        }
    }

}