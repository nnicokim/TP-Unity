using UnityEngine;

public class Zombie : MonoBehaviour, IInteractable, IDamageable
{
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
}
