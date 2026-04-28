using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour, IBullet
{
    #region PRIVATE_PROPERTIES
    [SerializeField] private IGun _owner;
    [SerializeField] private int _damage = 10;
    [SerializeField] private float _speed = 10;
    [SerializeField] private float _lifetime = 5;
    [SerializeField] private List<int> _layerMasks;
    #endregion

    #region I_BULLET_PROPERTIES
    public IGun Owner => _owner;
    public int Damage => _damage;
    public float Speed => _speed;
    public float LifeTime => _lifetime;
    #endregion

    #region I_BULLET_METHODS
    public void Travel() => transform.Translate(transform.forward * Time.deltaTime * Speed);

    public void OnCollisionEnter(Collision collision)
    {
        if (_layerMasks.Contains(collision.gameObject.layer))
        {
            IDamagable damagable = collision.gameObject.GetComponent<IDamagable>();
            if (damagable != null) EventQueueManager.Instance.AddEvent(new CmdApplyDamage(damagable, _damage));

            Destroy(this.gameObject);
        }
    }
    #endregion

    #region UNITY_EVENTS
    void Start()
    {
        // Rename a BulletDamage
        _damage = _owner.Damage;
        // Agregar BulletSpeed en stats
    }

    void Update()
    {
        Travel();

        _lifetime -= Time.deltaTime;
        if (_lifetime <= 0) Destroy(this.gameObject);
    }
    #endregion

    public void SetOwner(IGun owner) => _owner = owner;
}