using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Zombie : MonoBehaviour, IInteractable, IDamageable
{
    private const float DESTROY_AFTER_DEATH_DELAY = 1.5f;

    #region TARGET_GROUP
    [SerializeField] protected Transform _target;
    #endregion

    #region MOVEMENT_GROUP
    [SerializeField] protected float _moveSpeed = 2f;
    [SerializeField] protected float _rotationSpeed = 8f;
    [SerializeField] protected float _stopDistance = 1.4f;
    [SerializeField] protected float _detectionRange = 25f;
    #endregion

    #region ANIMATION_GROUP
    [SerializeField] protected Animation _animation;
    [SerializeField] protected string _walkAnimationName;
    [SerializeField] protected string _idleAnimationName;
    [SerializeField] protected string _attackAnimationName;
    [SerializeField] protected string _dieAnimationName;
    #endregion

    #region AUDIO_GROUP
    [SerializeField] protected AudioSource _audioSource;
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

    public virtual void Interact(Collider collider)
    {
        if (_isDead || !_canDamage)
            return;
        
        if (collider.gameObject.layer == this.gameObject.layer)
        {
            ApplyMovement(-DirectionToTarget(collider.gameObject.transform), lastSpeed);
            return;
        }
    }

    private void TryDealDamage(IDamageable lifeStrategy, string targetName)
    {
        if (_isDead || !_canDamage)
            return;

        if (lifeStrategy == null || ReferenceEquals(lifeStrategy, this))
            return;

        _canDamage = false;

        if (EventQueueManager.instance != null)
            EventQueueManager.instance.AddCommand(new CmdApplyDamage(lifeStrategy, Value));
        else
            lifeStrategy.ApplyDamage(Value);

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

    public void ApplyDamage(int damage)
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
        //ConfigureRigidbody();
        ResolveAnimation();
        ResolveAudio();
        SpawnState = new ZombieStateSpawn(this, _idleAnimationName, _idleClips, _StateMachine);
        IdleState = new ZombieStateIdle(this, _idleAnimationName, _idleClips, _StateMachine);
        ChaseState = new ZombieStateChase(this, _walkAnimationName, _idleClips, _StateMachine);
        AttackState = new ZombieStateAttack(this, _attackAnimationName, _attackClips, _StateMachine);
        HurtState = new ZombieStateHurt(this, _idleAnimationName, _hurtClips, _StateMachine);
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
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
            return;

        rb.includeLayers = 0;
        rb.excludeLayers = 0;
    }
    #endregion

    #region CHASE_GROUP

    public bool IsTargetInChaseRange()
    {
        Vector3 direction = _target.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;
        return _detectionRange <= 0f || distance <= _detectionRange;
    }

    public bool IsTargetInAttackRange()
    {
        Vector3 direction = _target.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;
        return distance <= _stopDistance;
    }

    protected Vector3 DirectionToTarget(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        return direction.normalized;
    }

    protected void ApplyMovement(Vector3 moveDirection, float speed)
    {
        transform.position += speed * Time.deltaTime * moveDirection;
        lastSpeed = speed;
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
        _animation.CrossFade(animationName);    
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
