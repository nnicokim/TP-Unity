using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spike : MonoBehaviour, IInteractable
{
    #region iINTERACTABLE_GROUP
    // Health recovery value
    public int Value => _damage;
    [SerializeField] private int _damage;

    public void Interact(Collider Collider)
    {
        Debug.Log($"Colision detectada con {Collider.name}");
        IDamageable lifeStrategy = Collider.GetComponentInParent<IDamageable>();
        lifeStrategy ??= Collider.GetComponentInChildren<IDamageable>();

        if (lifeStrategy != null && EventQueueManager.instance != null)
            EventQueueManager.instance.AddCommand(new CmdApplyDamage(lifeStrategy, Value));

        _collider.enabled = false;
        Invoke("EnableCollider", 2f);
        Debug.Log($"Daño aplicado: {Value}");
    }
    #endregion

    #region UNITY_EVENTS
    private Collider _collider;

    private void Start()
    {
        _collider = GetComponent<Collider>();
        if (_collider != null)
            _collider.isTrigger = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.includeLayers = 0;
            rb.excludeLayers = 0;
        }
    }

    private void OnTriggerEnter(Collider Collider) => Interact(Collider);
    private void OnTriggerStay(Collider Collider) => Interact(Collider);
    private void OnCollisionEnter(Collision collision) => Interact(collision.collider);

    private void EnableCollider()
    {
        if (_collider != null)
            _collider.enabled = true;
    }
    #endregion
}
