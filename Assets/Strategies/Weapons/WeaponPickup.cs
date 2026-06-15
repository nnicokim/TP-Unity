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

    [Header("Pickup Animation")]
    [SerializeField] private Animator _pickupAnimator;
    [SerializeField] private string _pistolHeldParameter = "pistolHeld";
    [SerializeField] private string _pistolHeldStateName = "PistolHeld";

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

        ResolvePickupAnimator();
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
        bool isUsingGenericDefault = IsApproximately(heldWeaponLocalScale, new Vector3(0.5f, 0.5f, 0.5f));
        bool isUsingWorldPrefabScale = IsApproximately(heldWeaponLocalScale, new Vector3(1.43f, 1.87f, 1.43f));

        if (!isUsingGenericDefault && !isUsingWorldPrefabScale)
            return;

        heldWeaponLocalPosition = new Vector3(0.65f, -0.7f, 1.15f);
        heldWeaponLocalEulerAngles = new Vector3(-90f, -93f, 0f);
        heldWeaponLocalScale = new Vector3(0.0048f, 0.0048f, 0.0048f);
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
        StopWorldPickupAnimation();
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

    private void ResolvePickupAnimator()
    {
        if (_pickupAnimator != null)
            return;

        if (weapon != null)
            _pickupAnimator = weapon.GetComponentInChildren<Animator>(true);

        if (_pickupAnimator == null)
            _pickupAnimator = GetComponentInChildren<Animator>(true);
    }

    public void StopWorldPickupAnimation()
    {
        if (weapon == null)
            weapon = GetComponentInChildren<Gun>(true);

        if (weapon == null)
            return;

        if (_pickupAnimator != null)
        {
            ApplyPistolHeldState(_pickupAnimator);
            return;
        }

        Animator[] animators = weapon.GetComponentsInChildren<Animator>(true);
        if (animators.Length == 0)
            animators = GetComponentsInChildren<Animator>(true);

        for (int i = 0; i < animators.Length; i++)
            ApplyPistolHeldState(animators[i]);

        Animation[] legacyAnimations = weapon.GetComponentsInChildren<Animation>(true);
        for (int i = 0; i < legacyAnimations.Length; i++)
            StopLegacyAnimation(legacyAnimations[i]);
    }

    private void ApplyPistolHeldState(Animator animator)
    {
        if (animator == null)
            return;

        if (!string.IsNullOrEmpty(_pistolHeldParameter))
            animator.SetBool(_pistolHeldParameter, true);

        if (!string.IsNullOrEmpty(_pistolHeldStateName))
        {
            animator.Play(_pistolHeldStateName, 0, 0f);
            animator.Update(0f);
        }

        animator.enabled = false;
    }

    private static void StopLegacyAnimation(Animation animation)
    {
        if (animation == null)
            return;

        animation.Stop();
        animation.enabled = false;
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
