using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class OxygenTank : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField, Min(1)] private int _life = 1;

    [Header("Explosion")]
    [SerializeField, Min(0)] private int _explosionDamage = 50;
    [SerializeField, Min(0.1f)] private float _explosionRadius = 4f;
    [SerializeField] private LayerMask _damageMask = ~0;
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField, Min(0f)] private float _explosionLifetime = 5f;

    private int _maxLife;
    private bool _hasExploded;

    public int Life => _life;
    public int MaxLife => _maxLife;

    private void Awake()
    {
        _maxLife = _life;

        if (GetComponent<Collider>() == null)
            Debug.LogWarning($"OxygenTank en {name} necesita un Collider para recibir disparos.", this);
    }

    public void ApplyDamage(int damage, DamageType type)
    {
        if (_hasExploded)
            return;

        _life -= damage;

        if (_life <= 0)
            Die();
    }

    public void ApplyHealthRecovery(int amount) { }

    public void Die()
    {
        if (_hasExploded)
            return;

        Explode();
    }

    private void Explode()
    {
        _hasExploded = true;

        SpawnExplosionVfx();
        ApplyExplosionDamage();

        Debug.Log($"Tanque de oxigeno {name} exploto.");
        Destroy(gameObject);
    }

    private void SpawnExplosionVfx()
    {
        if (_explosionPrefab == null)
        {
            Debug.LogWarning($"OxygenTank en {name} no tiene Explosion Prefab asignado.", this);
            return;
        }

        GameObject explosion = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);

        if (_explosionLifetime > 0f)
            Destroy(explosion, _explosionLifetime);
    }

    private void ApplyExplosionDamage()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            _explosionRadius,
            _damageMask,
            QueryTriggerInteraction.Collide);

        var damaged = new HashSet<IDamageable>();

        for (int i = 0; i < hits.Length; i++)
        {
            Collider hit = hits[i];
            Debug.Log(hit.name);
            if (hit == null || hit.transform.IsChildOf(transform))
                continue;

            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            damageable ??= hit.GetComponentInChildren<IDamageable>();

            if (damageable == null || !damaged.Add(damageable))
                continue;

            if (EventQueueManager.instance != null)
                EventQueueManager.instance.AddCommand(new CmdApplyDamage(damageable, _explosionDamage, DamageType.DAMAGE_EXPLOSION));
            else
                damageable.ApplyDamage(_explosionDamage, DamageType.DAMAGE_EXPLOSION);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.35f);
        Gizmos.DrawSphere(transform.position, _explosionRadius);
    }
#endif
}
