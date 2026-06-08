using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static InventoryManager;

[DisallowMultipleComponent]
public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private ItemWeapons weaponItem = ItemWeapons.PistolClip;
    [SerializeField] private bool autoDetectWeaponItem = true;
    [SerializeField, FormerlySerializedAs("pistol")] private Gun weapon;
    [SerializeField] private bool equipOnPickup = true;
    [SerializeField] private bool disablePickupColliders = true;
    [SerializeField] private bool makeWeaponCollidersTriggersOnStart = true;

    [Header("Held Pose")]
    [SerializeField] private bool applyHeldWeaponPose = true;
    [SerializeField] private Vector3 heldWeaponLocalPosition = new Vector3(0.7f, -1.2f, 1.5f);
    [SerializeField] private Vector3 heldWeaponLocalEulerAngles = new Vector3(-90f, -5f, 0f);
    [SerializeField] private Vector3 heldWeaponLocalScale = new Vector3(0.5f, 0.5f, 0.5f);

    [Header("Pickup Detection")]
    [SerializeField, Min(0.1f)] private float pickupRadius = 1.25f;
    [SerializeField] private LayerMask playerMask = ~0;
    [SerializeField] private bool checkPickupByDistance = true;

    [Header("Pickup Feedback")]
    [SerializeField] private Text weaponInstructionText;
    [SerializeField, HideInInspector, FormerlySerializedAs("gunInstructionText")] private Text legacyInstructionText;
    [SerializeField] private Text pickupText;
    [SerializeField] private string pickupMessage = "WeaponPickedUp";
    [SerializeField, Min(0f)] private float pickupMessageDuration = 2f;

    private bool _wasPickedUp;
    private CharacterInputManager _cachedPlayer;
    private Coroutine _pickupTextRoutine;

    protected virtual void Awake()
    {
        if (weapon == null)
            weapon = GetComponentInChildren<Gun>(true);

        AutoDetectWeaponItem();
        ConfigureWorldPickupColliders();
        SetInstructionVisible(true);
        SetPickupTextVisible(false);
    }

    private void AutoDetectWeaponItem()
    {
        if (!autoDetectWeaponItem || weapon == null)
            return;

        if (weapon is Rifle)
        {
            weaponItem = ItemWeapons.RifleClip;
            ApplyRifleHeldPoseIfUsingDefault();
        }
        else if (weapon is Pistol)
            weaponItem = ItemWeapons.PistolClip;
        else if (weapon is Shotgun)
        {
            weaponItem = ItemWeapons.ShotgunShell;
            ApplyShotgunHeldPoseIfUsingDefault();
        }
    }

    private void ApplyRifleHeldPoseIfUsingDefault()
    {
        if (!IsApproximately(heldWeaponLocalScale, new Vector3(0.5f, 0.5f, 0.5f)))
            return;

        heldWeaponLocalPosition = new Vector3(0.55f, -0.65f, 1.15f);
        heldWeaponLocalEulerAngles = new Vector3(-90f, 180f, 0f);
        heldWeaponLocalScale = new Vector3(0.0044f, 0.0033f, 0.0033f);
    }

    private bool IsApproximately(Vector3 a, Vector3 b)
    {
        return Mathf.Approximately(a.x, b.x)
            && Mathf.Approximately(a.y, b.y)
            && Mathf.Approximately(a.z, b.z);
    }

    private void ApplyShotgunHeldPoseIfUsingDefault()
    {
        if (!IsApproximately(heldWeaponLocalScale, new Vector3(0.5f, 0.5f, 0.5f)))
            return;

        heldWeaponLocalPosition = new Vector3(0.75f, -0.85f, 1.25f);
        heldWeaponLocalEulerAngles = new Vector3(-90f, -93f, 0f);
        heldWeaponLocalScale = new Vector3(0.45f, 0.45f, 0.45f);
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

        if (weapon == null)
        {
            Debug.LogWarning($"WeaponPickup: no hay arma asignada para recoger en {gameObject.name}.", this);
            return;
        }

        _wasPickedUp = true;
        SetInstructionVisible(false);
        PreparePickedUpWeapon();
        character.PickupWeapon(
            weapon,
            weaponItem,
            equipOnPickup,
            applyHeldWeaponPose,
            heldWeaponLocalPosition,
            heldWeaponLocalEulerAngles,
            heldWeaponLocalScale);
        weapon.PlayReloadSoundOnce();
        ShowPickupText();
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

    private void ShowPickupText()
    {
        if (pickupText == null)
            return;

        if (_pickupTextRoutine != null)
            StopCoroutine(_pickupTextRoutine);

        _pickupTextRoutine = StartCoroutine(ShowPickupTextRoutine());
    }

    private IEnumerator ShowPickupTextRoutine()
    {
        pickupText.text = pickupMessage;
        SetPickupTextVisible(true);

        yield return new WaitForSeconds(pickupMessageDuration);

        SetPickupTextVisible(false);
        _pickupTextRoutine = null;
    }

    private void SetPickupTextVisible(bool isVisible)
    {
        if (pickupText != null)
            pickupText.gameObject.SetActive(isVisible);
    }

    private void SetInstructionVisible(bool isVisible)
    {
        if (weaponInstructionText != null)
            weaponInstructionText.gameObject.SetActive(isVisible);

        if (legacyInstructionText != null)
            legacyInstructionText.gameObject.SetActive(isVisible);
    }

    private void ConfigureWorldPickupColliders()
    {
        if (!makeWeaponCollidersTriggersOnStart || weapon == null)
            return;

        Collider[] colliders = weapon.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].isTrigger = true;
    }

    private void PreparePickedUpWeapon()
    {
        Rigidbody rb = weapon.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (!disablePickupColliders)
            return;

        Collider[] colliders = weapon.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = false;
    }
}
