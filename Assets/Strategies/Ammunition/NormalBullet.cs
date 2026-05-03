using UnityEngine;

public class NormalBullet : MonoBehaviour, IBullet
{
    public float Speed => _speed;
    [SerializeField] private float _speed = 75f;

    public float LifeTime => _lifeTime;
    [SerializeField] private float _lifeTime = 4f;

    public Gun Owner => _owner;
    [SerializeField] private Gun _owner;

    private Vector3 _previousPosition;
    private bool _hasHit;

    public void Travel() => transform.Translate(Vector3.forward * Speed * Time.deltaTime);

    public void SetOwner(Gun owner) => _owner = owner;

    private void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    private void Start()
    {
        _previousPosition = transform.position;
    }

    private void Update()
    {
        if (_hasHit)
            return;

        Travel();
        SweepDetect();

        _lifeTime -= Time.deltaTime;
        if (_lifeTime <= 0) Destroy(gameObject);
    }

    // Raycast desde la posición anterior hasta la actual para evitar tunneling
    private void SweepDetect()
    {
        Vector3 currentPosition = transform.position;
        Vector3 delta = currentPosition - _previousPosition;
        float distance = delta.magnitude;

        if (distance > 0f && Physics.Raycast(_previousPosition, delta.normalized, out RaycastHit hit, distance, ~0, QueryTriggerInteraction.Collide))
        {
            ResolveHit(hit.collider);
        }

        _previousPosition = currentPosition;
    }

    private void OnCollisionEnter(Collision collision) => ResolveHit(collision.collider);
    private void OnTriggerEnter(Collider other) => ResolveHit(other);

    private void ResolveHit(Collider collider)
    {
        if (_hasHit || collider == null)
            return;

        // No dañar al dueño del arma (ej. el propio jugador) ni a otras balas
        if (_owner != null && collider.transform.IsChildOf(_owner.transform.root))
            return;

        if (collider.GetComponent<IBullet>() != null)
            return;

        IDamageable lifeStrategy = collider.GetComponentInParent<IDamageable>();
        lifeStrategy ??= collider.GetComponentInChildren<IDamageable>();

        if (lifeStrategy != null)
        {
            int damage = _owner != null ? _owner.Damage : 0;
            Debug.Log($"Bullet golpeo a {collider.name} (-{damage})");

            if (EventQueueManager.instance != null)
                EventQueueManager.instance.AddCommand(new CmdApplyDamage(lifeStrategy, damage));
            else
                lifeStrategy.ApplyDamage(damage);
        }

        _hasHit = true;
        Destroy(gameObject);
    }
}
