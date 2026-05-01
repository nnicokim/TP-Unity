using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBullet : MonoBehaviour, IBullet
{
    public float Speed => _speed;
    [SerializeField] private float _speed = 75f;

    public float LifeTime => _lifeTime;
    [SerializeField] private float _lifeTime = 4f;

    public Gun Owner => _owner;
    [SerializeField] private Gun _owner;

    public void Travel() => transform.Translate(Vector3.forward * Speed * Time.deltaTime);

    public void SetOwner(Gun owner) => _owner = owner;

    private void Update()
    {
        Travel();

        _lifeTime -= Time.deltaTime;
        if (_lifeTime <= 0) Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Bullet collide with: {collision.gameObject.name}");

        // 1. Detectar si el objeto colisionado tiene capacidad de manejar vida (obtener referncia)
        IDamageable lifeStrategy = collision.gameObject.GetComponentInParent<IDamageable>();
        lifeStrategy ??= collision.gameObject.GetComponentInChildren<IDamageable>();

        // 2. Preguntar si la referencia es nula o valida
        if (lifeStrategy != null)
        {
            // 3. Realizar la resta de vida
            if (EventQueueManager.instance != null)
                EventQueueManager.instance.AddCommand(new CmdApplyDamage(lifeStrategy, _owner.Damage));
            else
                lifeStrategy.ApplyDamage(_owner.Damage);
        }

        // 4. Matar la bala
        Destroy(gameObject);
    }
}
