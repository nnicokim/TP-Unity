using UnityEngine;

public class BossZombie : Zombie
{
    [Header("Exit Unlock")]
    [SerializeField] private ExitDoor _exitDoor;
    [SerializeField] private GameObject _exitKeyPrefab;
    [SerializeField] private Vector3 _keySpawnOffset = new Vector3(0f, 1f, 0f);

    [SerializeField] private float DashAttackRange;
    [SerializeField] protected float InitialDashSpeed;
    [SerializeField] public float DashWallDistance;
    [SerializeField] public float DashLength;

    [SerializeField] protected string _dashAnimationName;
    [SerializeField] protected AudioClip[] _dashClips;

    public StateMachineState DashState { get; protected set; }
    public StateMachineState AttackDashState { get; protected set; }

    private System.Collections.IEnumerator DashCoroutine;

    public override void ApplyDamage(int damage, DamageType type)
    {
        if (type == DamageType.DAMAGE_EXPLOSION)
            base.ApplyDamage(damage, type);
        else
            Debug.Log($"Damage from {type} for {damage} ignored");
    }

    protected override void OnDie()
    {
        if (_exitKeyPrefab == null)
        {
            Debug.LogError("BossZombie: falta asignar el prefab de llave (key).", this);
            return;
        }

        Vector3 spawnPosition = transform.position + _keySpawnOffset;
        GameObject keyObject = Instantiate(_exitKeyPrefab, spawnPosition, Quaternion.identity);

        ExitKeyPickup keyPickup = keyObject.GetComponent<ExitKeyPickup>();
        if (keyPickup == null)
            keyPickup = keyObject.GetComponentInChildren<ExitKeyPickup>();

        if (keyPickup != null)
            keyPickup.SetExitDoor(_exitDoor);
        else
            Debug.LogError("BossZombie: el prefab de llave no tiene ExitKeyPickup.", this);
    }

    public bool IsTargetInDashAttackRange()
    {
        float distance = DistanceToTarget(_target).magnitude;
        return distance <= DashAttackRange + 2 && distance >= DashAttackRange - 2;
    }

    public bool IsSafeDashDistance()
    {
        LayerMask layerMask = LayerMask.GetMask("Default");
        return !Physics.Raycast(transform.position, -DirectionToTarget(_target), DashWallDistance, layerMask);
    }

    public void StartDash(bool aggro)
    {
        Vector3 direction = DirectionToTarget(_target);
        if (aggro)
            DashCoroutine = DashAggro(direction);
        else
            DashCoroutine = DashAway(direction);

        StartCoroutine(DashCoroutine);
    }

    public void StopDash()
    {
        if (DashCoroutine != null)
            StopCoroutine(DashCoroutine);
    }

    private System.Collections.IEnumerator DashAggro(Vector3 direction)
    {
        float dashSpeed = InitialDashSpeed / 2;
        while (dashSpeed >= 0)
        {
            ApplyMovement(direction, dashSpeed);
            yield return null;
        }
    }

    private System.Collections.IEnumerator DashAway(Vector3 direction)
    {
        float dashSpeed = InitialDashSpeed;
        while (dashSpeed >= 0)
        {
            ApplyMovement(-direction, dashSpeed);
            dashSpeed -= dashSpeed * Unity.Mathematics.math.sqrt(Time.deltaTime / DashLength);
            yield return null;
        }

        DashCoroutine = null;
    }
}
