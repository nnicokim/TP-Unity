using UnityEngine;

public class Zombie : MonoBehaviour, IInteractable, IDamageable
{
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
    [SerializeField] private string _attackAnimationName = "attack";
    #endregion

    #region IINTERACTABLE_GROUP
    public int Value => _damage;
    [SerializeField] private int _damage = 10;

    [SerializeField] private float _damageCooldown = 1f;
    private bool _canDamage = true;

    public void Interact(Collider Collider)
    {
        if (!_canDamage)
            return;

        IDamageable lifeStrategy = Collider.GetComponentInParent<IDamageable>();
        lifeStrategy ??= Collider.GetComponentInChildren<IDamageable>();

        if (lifeStrategy == null || ReferenceEquals(lifeStrategy, this))
            return;

        _canDamage = false;

        if (EventQueueManager.instance != null)
            EventQueueManager.instance.AddCommand(new CmdApplyDamage(lifeStrategy, Value));
        else
            lifeStrategy.ApplyDamage(Value);

        Debug.Log($"Zombie aplico daño: {Value} a {Collider.name}");
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
        _life -= damage;
        Debug.Log($"Zombie recibio daño: {damage}. Vida restante: {_life}");

        if (_life <= 0)
            Die();
    }

    public void ApplyHealthRecovery(int amount)
    {
        _life = Mathf.Min(_life + amount, MaxLife);
    }

    public void Die()
    {
        Debug.Log($"Zombie {name} ha muerto.");
        Destroy(gameObject);
    }
    #endregion

    #region UNITY_EVENTS
    private void Start()
    {
        _maxLife = _life;
        ConfigureRigidbody();
        ResolveAnimation();
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
}
