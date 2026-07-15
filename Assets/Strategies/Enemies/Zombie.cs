using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Animation))]
public class Zombie : MonoBehaviour, IInteractable, IDamageable
{
    private const float DESTROY_AFTER_DEATH_DELAY = 1.5f;
    private const int MOVEMENT_CAST_HIT_COUNT = 8;

    #region TARGET_GROUP
    [SerializeField] protected Transform _target;
    #endregion

    #region MOVEMENT_GROUP
    [SerializeField] protected float _moveSpeed = 2f;
    [SerializeField] protected float _rotationSpeed = 8f;
    [SerializeField] protected float _stopDistance = 1.4f;
    [SerializeField] protected float _detectionRange = 25f;
    [SerializeField, Min(0f)] private float _movementSkinWidth = 0.05f;
    #endregion

    #region ANIMATION_GROUP
    protected Animation _animation;
    [SerializeField] protected string _walkAnimationName;
    [SerializeField] protected string _idleAnimationName;
    [SerializeField] protected string _attackAnimationName;
    [SerializeField] protected string _hurtAnimationName;
    [SerializeField] protected string _dieAnimationName;
    #endregion

    #region AUDIO_GROUP
    protected AudioSource _audioSource;
    [SerializeField] protected AudioClip[] _attackClips;
    [SerializeField] protected AudioClip[] _hurtClips;
    [SerializeField] protected AudioClip[] _dieClips;
    [SerializeField] protected AudioClip[] _idleClips;
    [SerializeField] protected float _idleSoundMinInterval = 4f;
    [SerializeField] protected float _idleSoundMaxInterval = 9f;
    private IEnumerator soundLoopCoroutine;
    #endregion

    #region STATE_MACHINE_GROUP
    protected StateMachine _StateMachine = new();
    public StateMachineState SpawnState { get; protected set; }
    public StateMachineState IdleState { get; protected set; }
    public StateMachineState ChaseState { get; protected set; }
    public StateMachineState AttackState { get; protected set; }
    public StateMachineState HurtState { get; protected set; }
    public StateMachineState DieState { get; protected set; }

    #endregion

    #region IINTERACTABLE_GROUP
    public int Value => _damage;
    [SerializeField] private int _damage = 25;

    [SerializeField] private float _damageCooldown = 2f;
    private bool _canDamage = true;
    private bool _isDead;

    protected float lastSpeed;
    private Rigidbody _rigidbody;
    private Collider _movementCollider;
    private readonly RaycastHit[] _movementHits = new RaycastHit[MOVEMENT_CAST_HIT_COUNT];
    private bool UsesKinematicCollisionMovement => GetType() == typeof(Zombie);

    public virtual void Interact(Collider collider)
    {
        if (_isDead || !_canDamage)
            return;
        
        if (collider.gameObject.layer == this.gameObject.layer)
            ApplyMovement(-DirectionToTarget(collider.gameObject.transform), lastSpeed);
        //else
            //ApplyMovement(-DirectionToTarget(collider.gameObject.transform), 1);

    }

    private void TryDealDamage(IDamageable lifeStrategy, string targetName)
    {
        if (_isDead || !_canDamage)
            return;

        if (lifeStrategy == null || ReferenceEquals(lifeStrategy, this))
            return;

        _canDamage = false;

        if (EventQueueManager.instance != null)
            EventQueueManager.instance.AddCommand(new CmdApplyDamage(lifeStrategy, Value, DamageType.DAMAGE_ZOMBIE));
        else
            lifeStrategy.ApplyDamage(Value, DamageType.DAMAGE_ZOMBIE);

        PlayRandomClip(_attackClips, true);

        Debug.Log($"Zombie aplico daño: {Value} a {targetName}");
        Invoke(nameof(EnableDamage), _damageCooldown);
    }
    #endregion

    #region IDAMAGEABLE_GROUP
    public int Life => _life;
    [SerializeField] private int _life = 100;

    public int MaxLife => _maxLife;
    private int _maxLife;

    public virtual void ApplyDamage(int damage, DamageType damageType)
    {
        if (_isDead)
            return;

        _life -= damage;
        Debug.Log($"Zombie recibio daño: {damage}. Vida restante: {_life}");

        if (_life <= 0)
            _StateMachine.ChangeState(DieState);
        else _StateMachine.ChangeState(HurtState);
    }

    public void ApplyHealthRecovery(int amount)
    {
        if (_isDead)
            return;

        _life = Mathf.Min(_life + amount, MaxLife);
    }

    public virtual void Die()
    {
        if (_isDead)
            return;

        _isDead = true;
        _canDamage = false;
        CancelInvoke(nameof(EnableDamage));
        GameplayStatsManager.RegisterZombieKilled();
        OnDie();

        Debug.Log($"Zombie {name} ha muerto.");
        Destroy(gameObject, DESTROY_AFTER_DEATH_DELAY);
    }

    protected virtual void OnDie() { }
    #endregion

    #region UNITY_EVENTS
    protected virtual void Start()
    {
        _maxLife = _life;
        ConfigureRigidbody();
        ResolveAnimation();
        ResolveAudio();
        SpawnState = new ZombieStateSpawn(this, _idleAnimationName, _idleClips, _StateMachine);
        IdleState = new ZombieStateIdle(this, _idleAnimationName, _idleClips, _StateMachine);
        ChaseState = new ZombieStateChase(this, _walkAnimationName, _idleClips, _StateMachine);
        AttackState = new ZombieStateAttack(this, _attackAnimationName, _attackClips, _StateMachine);
        HurtState = new ZombieStateHurt(this, _hurtAnimationName, _hurtClips, _StateMachine);
        DieState = new ZombieStateDie(this, _dieAnimationName, _dieClips, _StateMachine);
        _StateMachine.InitStateMachine(SpawnState);
    }

    private void Update()
    {
        _StateMachine.CurrentState.UpdateLogic();
    }

    private void OnTriggerEnter(Collider Collider) => Interact(Collider);
    private void OnTriggerStay(Collider Collider) => Interact(Collider);
    private void OnCollisionEnter(Collision collision) => Interact(collision.collider);
    private void OnCollisionStay(Collision collision) => Interact(collision.collider);

    private void EnableDamage() => _canDamage = true;

    private void ConfigureRigidbody()
    {
        if (!UsesKinematicCollisionMovement)
            return;

        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody == null)
            return;

        // Zombies are moved manually by their state machine, not by physics.
        // Keeping them dynamic lets the player collision launch them upward.
        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = false;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

        _movementCollider = GetComponent<Collider>();
        if (_movementCollider == null)
            _movementCollider = GetComponentInChildren<Collider>();
    }

    #endregion

    #region CHASE_GROUP

    public bool IsTargetInChaseRange()
    {
        float distance = DistanceToTarget(_target).magnitude;
        return _detectionRange <= 0f || distance <= _detectionRange;
    }

    public bool IsTargetInAttackRange()
    {
        float distance = DistanceToTarget(_target).magnitude;
        return distance <= _stopDistance;
    }

    protected Vector3 DistanceToTarget(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;
        return direction;
    }

    protected Vector3 DirectionToTarget(Transform target)
    {
        return DistanceToTarget(target).normalized;
    }

    protected void ApplyMovement(Vector3 moveDirection, float speed)
    {
        //if (!UsesKinematicCollisionMovement)
        {
            transform.position += speed * Time.deltaTime * moveDirection;
            lastSpeed = speed;
            return;
        }

        Vector3 displacement = GetAllowedDisplacement(moveDirection, speed);
        Vector3 nextPosition = transform.position + displacement;

        if (_rigidbody != null && _rigidbody.isKinematic)
            _rigidbody.MovePosition(nextPosition);
        else
            transform.position = nextPosition;

        lastSpeed = speed;
    }

    private Vector3 GetAllowedDisplacement(Vector3 moveDirection, float speed)
    {
        if (speed <= 0f || moveDirection.sqrMagnitude <= 0f)
            return Vector3.zero;

        moveDirection.y = 0f;
        if (moveDirection.sqrMagnitude <= 0f)
            return Vector3.zero;

        Vector3 direction = moveDirection.normalized;
        float desiredDistance = speed * Time.deltaTime;

        if (_movementCollider == null || desiredDistance <= 0f)
            return direction * desiredDistance;

        float castDistance = desiredDistance + _movementSkinWidth;
        int hitCount = CastMovementCollider(direction, castDistance);
        float allowedDistance = desiredDistance;
        Vector3 normalDirection = direction;

        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = _movementHits[i].collider;
            if (hitCollider == null || hitCollider.isTrigger || hitCollider.transform.IsChildOf(transform))
                continue;

            Vector3 hitDirection = (hitCollider.transform.position - transform.position).normalized;
            float distanceBeforeHit = Mathf.Max(0f, _movementHits[i].distance - _movementSkinWidth);
            allowedDistance = Mathf.Min(allowedDistance, distanceBeforeHit);
            if (allowedDistance == distanceBeforeHit)
                normalDirection = hitDirection;
        }

        int flip = Random.Range(0.0f, 1.0f) >= 0.5f ? 1 : -1;
        return direction * allowedDistance + Quaternion.AngleAxis(90 * flip, Vector3.up) * normalDirection * (desiredDistance - allowedDistance);
    }

    private int CastMovementCollider(Vector3 direction, float distance)
    {
        if (_movementCollider is BoxCollider boxCollider)
        {
            Vector3 center = boxCollider.transform.TransformPoint(boxCollider.center);
            Vector3 halfExtents = GetScaledHalfExtents(boxCollider);
            Quaternion orientation = boxCollider.transform.rotation;

            return Physics.BoxCastNonAlloc(
                center,
                halfExtents,
                direction,
                _movementHits,
                orientation,
                distance,
                ~0,
                QueryTriggerInteraction.Ignore);
        }

        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        return Physics.RaycastNonAlloc(
            rayOrigin,
            direction,
            _movementHits,
            distance,
            ~0,
            QueryTriggerInteraction.Ignore);
    }

    private static Vector3 GetScaledHalfExtents(BoxCollider boxCollider)
    {
        Vector3 scale = boxCollider.transform.lossyScale;
        scale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        return Vector3.Scale(boxCollider.size * 0.5f, scale);
    }

    public void ChaseTarget()
    {
        // if (_target == null || IsGamePaused())
        // {
        //     PlayMovementAnimation(false);
        //     return;
        // }
        Vector3 direction = DirectionToTarget(_target);
        RotateTowards(direction);
        ApplyMovement(direction, _moveSpeed);
    }

    private void RotateTowards(Vector3 direction)
    {
        if (direction == Vector3.zero)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 0f, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }

    //Moves slowly in a random direction
    public void WalkAround()
    {
        //TODO
    }

    private bool IsGamePaused()
    {
        return GameManager.instance != null && GameManager.instance.isGamePause;
    }

    // El daño no depende de que los colliders se toquen: si el target está dentro de
    // _stopDistance y el cooldown está listo, aplicamos daño.
    public void AttackTargetInRange()
    {
        if (_target == null)
            return;

        IDamageable lifeStrategy = _target.GetComponentInParent<IDamageable>();
        lifeStrategy ??= _target.GetComponentInChildren<IDamageable>();

        TryDealDamage(lifeStrategy, _target.name);
    }
    #endregion

    #region ANIMATION_PLAYBACK_GROUP
    private void ResolveAnimation()
    {
        if (_animation == null)
            _animation = GetComponentInChildren<Animation>();
    }

    public void PlayAnimation(string animationName, bool isOneshot)
    {
        if (string.IsNullOrEmpty(animationName) || _animation.IsPlaying(animationName))
            return;

        if (isOneshot) _animation.wrapMode = WrapMode.Once;
        else _animation.wrapMode = WrapMode.Loop;
        _animation.CrossFade(animationName, 0.1f);    
    }
    #endregion

    #region AUDIO_PLAYBACK_GROUP
    private void ResolveAudio()
    {
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();
    }

    public void PlayRandomClip(AudioClip[] clips, bool isOneshot)
    {
        if (_audioSource == null || clips == null || clips.Length == 0)
            return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        _audioSource.PlayOneShot(clip);
        if (!isOneshot)
            soundLoopCoroutine = ScheduleNextIdleSound(clips);
            StartCoroutine(ScheduleNextIdleSound(clips));
    }

    private IEnumerator ScheduleNextIdleSound(AudioClip[] clips)
    {
        float delay = Random.Range(_idleSoundMinInterval, Mathf.Max(_idleSoundMinInterval, _idleSoundMaxInterval));
        yield return new WaitForSeconds(delay);
        if (clips == null || clips.Length == 0)
            yield break;

        PlayRandomClip(clips, false);
        yield break;
    }

    public void StopAudioclips()
    {
        StopCoroutine(soundLoopCoroutine);
    }
    #endregion
}
