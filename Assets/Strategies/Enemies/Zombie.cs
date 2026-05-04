using UnityEngine;

public class Zombie : MonoBehaviour, IInteractable, IDamageable
{
    private const float DESTROY_AFTER_DEATH_DELAY = 1.5f;

    #region TARGET_GROUP
    [SerializeField] private Transform _target;
    #endregion

    #region MOVEMENT_GROUP
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _rotationSpeed = 8f;
    [SerializeField] private float _stopDistance = 1.4f;
    [SerializeField] private float _detectionRange = 25f;
    #endregion

    #region ANIMATION_GROUP
    [SerializeField] private Animation _animation;
    [SerializeField] private string _walkAnimationName;
    [SerializeField] private string _idleAnimationName;
    [SerializeField] private string _attackAnimationName;
    [SerializeField] private string _dieAnimationName;
    #endregion

    #region AUDIO_GROUP
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip[] _attackClips;
    [SerializeField] private AudioClip[] _hurtClips;
    [SerializeField] private AudioClip[] _dieClips;
    [SerializeField] private AudioClip[] _idleClips;
    [SerializeField] private float _idleSoundMinInterval = 4f;
    [SerializeField] private float _idleSoundMaxInterval = 9f;
    #endregion

    #region IINTERACTABLE_GROUP
    public int Value => _damage;
    [SerializeField] private int _damage = 25;

    [SerializeField] private float _damageCooldown = 2f;
    private bool _canDamage = true;
    private bool _isDead;

    public void Interact(Collider Collider)
    {
        if (_isDead || !_canDamage)
            return;

        IDamageable lifeStrategy = Collider.GetComponentInParent<IDamageable>();
        lifeStrategy ??= Collider.GetComponentInChildren<IDamageable>();

        TryDealDamage(lifeStrategy, Collider != null ? Collider.name : "<unknown>");
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

        PlayRandomClip(_attackClips);

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
        {
            Die();
            return;
        }

        PlayRandomClip(_hurtClips);
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
        CancelInvoke(nameof(PlayIdleSound));
        OnDie();

        Debug.Log($"Zombie {name} ha muerto.");
        PlayDieAnimation();
        PlayRandomClip(_dieClips);
        Destroy(gameObject, DESTROY_AFTER_DEATH_DELAY);
    }

    protected virtual void OnDie() { }
    #endregion

    #region UNITY_EVENTS
    private void Start()
    {
        _maxLife = _life;
        ConfigureRigidbody();
        ResolveAnimation();
        ResolveAudio();
        ScheduleNextIdleSound();
    }

    private void Update()
    {
        ChaseTarget();
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

    private void OnValidate()
    {
        _life = Mathf.Max(1, _life);
        _damage = Mathf.Max(0, _damage);
        _damageCooldown = Mathf.Max(0.1f, _damageCooldown);
        _moveSpeed = Mathf.Max(0f, _moveSpeed);
        _rotationSpeed = Mathf.Max(0f, _rotationSpeed);
        _stopDistance = Mathf.Max(0f, _stopDistance);
        _detectionRange = Mathf.Max(0f, _detectionRange);
    }
    #endregion

    #region CHASE_GROUP
    private void ChaseTarget()
    {
        if (_isDead)
            return;

        if (_target == null || IsGamePaused())
        {
            PlayMovementAnimation(false);
            return;
        }

        Vector3 direction = _target.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;
        bool targetDetected = _detectionRange <= 0f || distance <= _detectionRange;
        if (!targetDetected)
        {
            PlayMovementAnimation(false);
            return;
        }

        Vector3 moveDirection = direction.normalized;

        if (distance <= _stopDistance)
        {
            RotateTowards(moveDirection);
            PlayAttackAnimation();
            AttackTargetInRange();
            return;
        }

        if (_moveSpeed <= 0f)
        {
            PlayMovementAnimation(false);
            return;
        }

        RotateTowards(moveDirection);
        transform.position += moveDirection * _moveSpeed * Time.deltaTime;
        PlayMovementAnimation(true);
    }

    private void RotateTowards(Vector3 direction)
    {
        if (direction == Vector3.zero)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 0f, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }

    private bool IsGamePaused()
    {
        return GameManager.instance != null && GameManager.instance.isGamePause;
    }

    // El daño no depende de que los colliders se toquen: si el target está dentro de
    // _stopDistance y el cooldown está listo, aplicamos daño.
    private void AttackTargetInRange()
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

    private void PlayMovementAnimation(bool isMoving)
    {
        if (_animation == null)
            return;

        if (isMoving)
        {
            PlayAnimation(GetAnimationName(_walkAnimationName));
            return;
        }

        if (!string.IsNullOrEmpty(_idleAnimationName))
            PlayAnimation(_idleAnimationName);
        else
            _animation.Stop();
    }

    private void PlayAttackAnimation()
    {
        if (_animation == null)
            return;

        if (!string.IsNullOrEmpty(_attackAnimationName))
            PlayAnimation(_attackAnimationName);
        else
            PlayMovementAnimation(false);
    }

    private void PlayDieAnimation()
    {
        if (_animation == null)
            return;

        if (!string.IsNullOrEmpty(_dieAnimationName))
            PlayAnimation(_dieAnimationName);
        else
            _animation.Stop();
    }

    private string GetAnimationName(string preferredName)
    {
        if (!string.IsNullOrEmpty(preferredName))
            return preferredName;

        return _animation.clip != null ? _animation.clip.name : null;
    }

    private void PlayAnimation(string animationName)
    {
        if (string.IsNullOrEmpty(animationName) || _animation.IsPlaying(animationName))
            return;

        _animation.CrossFade(animationName);
    }
    #endregion

    #region AUDIO_PLAYBACK_GROUP
    private void ResolveAudio()
    {
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();
    }

    private void PlayRandomClip(AudioClip[] clips)
    {
        if (_audioSource == null || clips == null || clips.Length == 0)
            return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip != null)
            _audioSource.PlayOneShot(clip);
    }

    private void ScheduleNextIdleSound()
    {
        if (_idleClips == null || _idleClips.Length == 0)
            return;

        float delay = Random.Range(_idleSoundMinInterval, Mathf.Max(_idleSoundMinInterval, _idleSoundMaxInterval));
        Invoke(nameof(PlayIdleSound), delay);
    }

    private void PlayIdleSound()
    {
        if (_isDead)
            return;

        PlayRandomClip(_idleClips);
        ScheduleNextIdleSound();
    }
    #endregion
}
