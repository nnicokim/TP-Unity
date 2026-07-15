using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class BossZombie : Zombie
{
    [SerializeField] private ExitDoor _exitDoor;
    [SerializeField] private float DashAttackRange;
    [SerializeField] protected float InitialDashSpeed;
    [SerializeField] public float DashWallDistance;
    [SerializeField] public float DashLength;

    [SerializeField] protected string _dashAnimationName;
    [SerializeField] protected AudioClip[] _dashClips;

    public StateMachineState DashState { get; protected set; }
    public StateMachineState AttackDashState { get; protected set; }

    private IEnumerator DashCoroutine;

    protected override void OnDie()
    {
        if (_exitDoor != null)
            _exitDoor.Unlock();
    }

    public bool IsTargetInDashAttackRange()
    {
        float distance = DistanceToTarget(_target).magnitude;
        return distance <= DashAttackRange + 2 && distance >= DashAttackRange - 2;
    }

    public bool IsSafeDashDistance()
    {
        LayerMask layerMask;
        layerMask = LayerMask.GetMask("Default");
        return !Physics.Raycast(transform.position, -DirectionToTarget(_target), DashWallDistance, layerMask);
    }
    public void StartDash(bool aggro)
    {
        Vector3 direction = DirectionToTarget(_target);
        if (aggro) DashCoroutine = DashAggro(direction);
        else DashCoroutine = DashAway(direction);
        StartCoroutine(DashCoroutine);
    }

    public void StopDash()
    {
        if (DashCoroutine != null) StopCoroutine(DashCoroutine);
    }

    private IEnumerator DashAggro(Vector3 direction)
    {
        float dashSpeed = InitialDashSpeed/2;
        while (dashSpeed >= 0)
        {
            ApplyMovement(direction, dashSpeed);
            yield return null;
        }
        yield break;
    }

    private IEnumerator DashAway(Vector3 direction)
    {
        float dashSpeed = InitialDashSpeed;
        while (dashSpeed >= 0)
        {
            ApplyMovement(-direction, dashSpeed);
            dashSpeed -= dashSpeed * math.sqrt(Time.deltaTime / DashLength);
            yield return null;
        }
        DashCoroutine = null;
        yield break;
    }
}
