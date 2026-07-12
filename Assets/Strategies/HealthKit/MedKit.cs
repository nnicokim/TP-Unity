using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedKit : MonoBehaviour, IInteractable
{
    #region iINTERACTABLE_GROUP
    // Health recovery value
    public int Value => _healthRecoveryValue;
    [SerializeField] private int _healthRecoveryValue;

    [Header("World Pickup Rotation")]
    [SerializeField] private bool rotateWhenAvailable = true;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField, Min(0f)] private float rotationSpeed = 90f;
    [SerializeField] private Transform visualRoot;

    private bool _canInteract = true;

    public void Interact(Collider Collider)
    {
        if (!_canInteract)
            return;

        _canInteract = false;

        Debug.Log($"Colision detectada con {Collider.name}");
        IDamageable lifeStrategy = Collider.GetComponentInParent<IDamageable>();
        lifeStrategy ??= Collider.GetComponentInChildren<IDamageable>();

        if (lifeStrategy != null && EventQueueManager.instance != null)
            EventQueueManager.instance.AddCommand(new CmdApplyHealth(lifeStrategy, Value));

        _collider.enabled = false;
        Invoke("EnableCollider", 3f);
        Debug.Log($"Vida recuperada: {Value}");
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

    private void Update()
    {
        RotatePickup();
    }

    private void OnTriggerEnter(Collider Collider) => Interact(Collider);
    private void OnTriggerStay(Collider Collider) => Interact(Collider);
    private void OnCollisionEnter(Collision collision) => Interact(collision.collider);

    private void EnableCollider()
    {
        _canInteract = true;

        if (_collider != null)
            _collider.enabled = true;
    }

    private void RotatePickup()
    {
        if (!rotateWhenAvailable || HasRunningPickupAnimation() || rotationSpeed <= 0f)
            return;

        Vector3 axis = rotationAxis.sqrMagnitude > 0f ? rotationAxis.normalized : Vector3.up;
        Transform target = visualRoot != null ? visualRoot : transform;
        target.Rotate(axis, rotationSpeed * Time.deltaTime, Space.World);
    }

    private bool HasRunningPickupAnimation()
    {
        Animator[] animators = GetComponentsInChildren<Animator>(true);
        for (int i = 0; i < animators.Length; i++)
        {
            Animator animator = animators[i];
            if (animator != null && animator.enabled && animator.runtimeAnimatorController != null)
                return true;
        }

        Animation[] animations = GetComponentsInChildren<Animation>(true);
        for (int i = 0; i < animations.Length; i++)
        {
            Animation animation = animations[i];
            if (animation != null && animation.enabled && animation.isPlaying)
                return true;
        }

        return false;
    }
    #endregion
}
