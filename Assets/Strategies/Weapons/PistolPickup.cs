using UnityEngine;

[DisallowMultipleComponent]
public class PistolPickup : MonoBehaviour
{
    [SerializeField] private Gun pistol;
    [SerializeField] private bool disablePickupColliders = true;

    [Header("Pickup Detection")]
    [SerializeField, Min(0.1f)] private float pickupRadius = 1.25f;
    [SerializeField] private LayerMask playerMask = ~0;
    [SerializeField] private bool checkPickupByDistance = true;

    private bool _wasPickedUp;
    private CharacterInputManager _cachedPlayer;

    private void Awake()
    {
        if (pistol == null)
            pistol = GetComponentInChildren<Gun>(true);
    }

    private void Update()
    {
        if (_wasPickedUp || !checkPickupByDistance)
            return;

        TryPickupByDistance();
    }

    private void OnTriggerEnter(Collider other)
    {
        TryPickup(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryPickup(collision.collider);
    }

    private void TryPickup(Collider other)
    {
        if (_wasPickedUp || other == null)
            return;

        CharacterInputManager character = other.GetComponentInParent<CharacterInputManager>();
        if (character == null)
            character = other.GetComponentInChildren<CharacterInputManager>();

        TryPickup(character);
    }

    private void TryPickup(CharacterInputManager character)
    {
        if (_wasPickedUp || character == null)
            return;

        if (pistol == null)
        {
            Debug.LogWarning("PistolPickup: no hay una pistola asignada para recoger.", this);
            return;
        }

        _wasPickedUp = true;
        PreparePickedUpPistol();
        character.PickupPistol(pistol);
    }

    private void TryPickupByDistance()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius, playerMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < hits.Length; i++)
        {
            CharacterInputManager character = hits[i].GetComponentInParent<CharacterInputManager>();
            if (character == null)
                character = hits[i].GetComponentInChildren<CharacterInputManager>();

            if (character == null)
                continue;

            TryPickup(character);
            return;
        }

        if (_cachedPlayer == null)
            _cachedPlayer = FindFirstObjectByType<CharacterInputManager>();

        if (_cachedPlayer == null)
            return;

        float sqrDistance = (transform.position - _cachedPlayer.transform.position).sqrMagnitude;
        if (sqrDistance <= pickupRadius * pickupRadius)
            TryPickup(_cachedPlayer);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }

    private void PreparePickedUpPistol()
    {
        Rigidbody rb = pistol.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (!disablePickupColliders)
            return;

        Collider[] colliders = pistol.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = false;
    }
}
