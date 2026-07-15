using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static InventoryManager;

public class CharacterInputManager : MonoBehaviour
{
    private enum WeaponMode
    {
        Fists,
        Gun
    }

    // KEY BINDINGS
    [SerializeField] private Key _keyForward = Key.W;
    [SerializeField] private Key _keyBack = Key.S;
    [SerializeField] private Key _keyLeft = Key.A;
    [SerializeField] private Key _keyRight = Key.D;
    [SerializeField] private Key _keyReload = Key.R;
    [SerializeField] private Key _keyFists = Key.Space;
    [SerializeField] private Key _keyWeapon1 = Key.Digit1;
    [SerializeField] private Key _keyWeapon2 = Key.Digit2;
    [SerializeField] private Key _keyWeapon3 = Key.Digit3;


    // MOVEMENT STRATEGIES
    private Walk _walk;
    private Run _run;

    // WEAPONS LIST
    private const int PISTOL_ID = 0; // TODO: ver como hacer si solo porta 2 armas.
    private const int RIFLE_ID = 1;
    private const int SHOTGUN_ID = 2;

    [SerializeField] private GameObject[] _weapons;
    [SerializeField] private Gun _equipedGun; // main gun strategy

    [Header("Fists / Weapon Switching")]
    [SerializeField] private Fists _fists;
    [SerializeField] private GameObject _fistsView;
    [SerializeField] private bool _startWithPistol;
    [SerializeField] private bool _hasPistol;
    [SerializeField] private bool _hasRifle;
    [SerializeField] private bool _hasShotgun;
    [SerializeField] private Transform _weaponHolder;
    [SerializeField] private bool _applyHeldWeaponPoseOnPickup = true;
    [SerializeField] private Vector3 _heldWeaponLocalPosition = new Vector3(1f, 1.38f, 0f);
    [SerializeField] private Vector3 _heldWeaponLocalEulerAngles = new Vector3(-90f, -5f, 0f);
    [SerializeField] private Vector3 _heldWeaponLocalScale = new Vector3(0.5f, 0.5f, 0.5f);

    [Header("Camera Relative Movement")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private bool _faceCameraDirection = true;
    [SerializeField] private float _rotationSmoothSpeed = 12f;

    // COMMANDS - WEAPONS
    private CmdAttack _cmdAttack;
    private CmdReload _cmdReload;
    private WeaponMode _weaponMode = WeaponMode.Fists;

    private void Start()
    {
        // Asignación de referencias
        _walk = GetComponent<Walk>();
        _run = GetComponent<Run>();

        if (_walk == null)
        {
            Debug.LogError($"Falta el componente Walk en {gameObject.name}.");
            enabled = false;
            return;
        }

        if (_run == null)
        {
            Debug.LogError($"Falta el componente Run en {gameObject.name}.");
            enabled = false;
            return;
        }

        if (EventQueueManager.instance == null)
        {
            Debug.LogError("No hay EventQueueManager en la escena.");
            enabled = false;
            return;
        }

        ResolveCameraTransform();
        ResolveFists();
        ResolveWeaponHolder();

        if (_weapons == null || _weapons.Length == 0)
            LoadWeaponsFromChildren();

        if (_startWithPistol && HasWeapon(PISTOL_ID))
        {
            _hasPistol = true;
            WeaponsSelection(ItemWeapons.PistolClip);
            return;
        }

        EquipFists();
    }

    private void EquipFists()
    {
        HideAllWeapons();
        SetFistsActive(true);

        _equipedGun = null;
        _cmdAttack = null;
        _cmdReload = null;
        _weaponMode = WeaponMode.Fists;

        if (ActionsManager.instance != null)
            ActionsManager.instance.ActionClearWeaponFeedback();
    }

    private void Update()
    {
        if (GameManager.instance != null && (GameManager.instance.isGameOver || GameManager.instance.isGamePause))
            return;

        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (keyboard == null)
            return;

        bool isRunning = keyboard.leftShiftKey.isPressed;

        HandleMovementInput(keyboard, isRunning);
        RotateTowardsCameraDirection();

        // WEAPONS
        if (mouse != null && mouse.leftButton.wasPressedThisFrame && !IsPointerOverUi())
        {
            HandleAttackInput();
        }

        if (keyboard[_keyReload].wasPressedThisFrame && _weaponMode == WeaponMode.Gun && _cmdReload != null)
        {
            EventQueueManager.instance.AddCommand(_cmdReload);
        }

        // WEAPONS SELECTION
        if (keyboard[_keyFists].wasPressedThisFrame)
        {
            EquipFists();
        }

        if (keyboard[_keyWeapon1].wasPressedThisFrame)
        {
            WeaponsSelection(ItemWeapons.PistolClip);
        }

        if (keyboard[_keyWeapon2].wasPressedThisFrame)
        {
            WeaponsSelection(ItemWeapons.RifleClip);
        }

        if (keyboard[_keyWeapon3].wasPressedThisFrame)
        {
            WeaponsSelection(ItemWeapons.ShotgunShell);
        }
    }

    private void WeaponsSelection(InventoryManager.ItemWeapons selection)
    {
        if (_weapons == null || _weapons.Length == 0)
            LoadWeaponsFromChildren();

        int weaponIndex = (int)selection;
        if (!HasOwnedWeapon(weaponIndex))
        {
            Debug.Log($"Todavia no tenes {selection}.");
            return;
        }

        if (_weapons == null || weaponIndex >= _weapons.Length || _weapons[weaponIndex] == null)
        {
            Debug.LogWarning($"No hay arma configurada para {selection}.");
            return;
        }

        HideAllWeapons();
        SetFistsActive(false);

        // 2. Activar el arma seleccionada
        _equipedGun = _weapons[weaponIndex].GetComponent<Gun>();
        if (_equipedGun == null)
        {
            Debug.LogWarning($"{_weapons[weaponIndex].name} no tiene componente Gun.");
            return;
        }

        EnsureEquippedWeaponUsesHolder(selection, _equipedGun.transform);
        _equipedGun.gameObject.SetActive(true);
        FitHeldWeaponIfNeeded(selection, _equipedGun.transform);
        _equipedGun.ResetVisualRecoilRestPose();

        // 3. Crear nuevas las estrategias
        _cmdAttack = new CmdAttack(_equipedGun);
        _cmdReload = new CmdReload(_equipedGun);
        _weaponMode = WeaponMode.Gun;

        // 4. Update Ui Feedback
        if (ActionsManager.instance != null)
            ActionsManager.instance.ActionWeaponChangeFeedback(selection);

        _equipedGun.RefreshAmmoUi();
    }

    public void PickupPistol(Gun pistol)
    {
        PickupWeapon(
            pistol,
            ItemWeapons.PistolClip,
            true,
            _applyHeldWeaponPoseOnPickup,
            _heldWeaponLocalPosition,
            _heldWeaponLocalEulerAngles,
            _heldWeaponLocalScale);
    }

    public void PickupWeapon(Gun weapon, ItemWeapons item, bool equipImmediately, bool applyHeldPose, Vector3 heldPosition, Vector3 heldEulerAngles, Vector3 heldScale)
    {
        if (weapon == null)
        {
            Debug.LogWarning($"CharacterInputManager: se intento recoger un arma null para {item}.", this);
            return;
        }

        int weaponIndex = (int)item;
        EnsureWeaponSlot(weaponIndex);
        _weapons[weaponIndex] = weapon.gameObject;
        SetWeaponOwned(weaponIndex, true);

        AttachWeaponToHolder(weapon.transform, applyHeldPose, heldPosition, heldEulerAngles, heldScale);

        if (equipImmediately)
            WeaponsSelection(item);

        Debug.Log($"{item} recogida.");
    }

    private void HandleAttackInput()
    {
        if (_weaponMode == WeaponMode.Fists)
        {
            if (_fists != null)
                _fists.Attack();
            else
                Debug.LogWarning("No hay Fists asignado al CharacterInputManager.", this);

            return;
        }

        if (_cmdAttack != null)
            EventQueueManager.instance.AddCommand(_cmdAttack);
    }

    private void HandleMovementInput(Keyboard keyboard, bool isRunning)
    {
        Vector3 moveDirection = GetCameraRelativeMoveDirection(keyboard);
        if (moveDirection == Vector3.zero)
            return;

        IMovable movementStrategy = isRunning ? _run : _walk;
        EventQueueManager.instance.AddCommand(new CmdMove(movementStrategy, moveDirection));
    }

    private Vector3 GetCameraRelativeMoveDirection(Keyboard keyboard)
    {
        ResolveCameraTransform();

        Vector3 forward = cameraTransform != null ? cameraTransform.forward : transform.forward;
        Vector3 right = cameraTransform != null ? cameraTransform.right : transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward = forward.sqrMagnitude > 0f ? forward.normalized : transform.forward;
        right = right.sqrMagnitude > 0f ? right.normalized : transform.right;

        Vector3 direction = Vector3.zero;

        if (keyboard[_keyForward].isPressed)
            direction += forward;

        if (keyboard[_keyBack].isPressed)
            direction -= forward;

        if (keyboard[_keyLeft].isPressed)
            direction -= right;

        if (keyboard[_keyRight].isPressed)
            direction += right;

        return direction.sqrMagnitude > 1f ? direction.normalized : direction;
    }

    private void RotateTowardsCameraDirection()
    {
        if (!_faceCameraDirection)
            return;

        ResolveCameraTransform();

        Vector3 lookDirection = cameraTransform != null ? cameraTransform.forward : transform.forward;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude <= 0f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSmoothSpeed * Time.deltaTime);
    }

    private void ResolveCameraTransform()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void ResolveFists()
    {
        if (_fists == null)
            _fists = GetComponentInChildren<Fists>(true);
    }

    private void ResolveWeaponHolder()
    {
        if (_weaponHolder != null)
            return;

        if (cameraTransform != null)
            _weaponHolder = cameraTransform;
        else if (Camera.main != null)
            _weaponHolder = Camera.main.transform;
    }

    private bool IsPointerOverUi()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private void LoadWeaponsFromChildren()
    {
        Gun[] guns = GetComponentsInChildren<Gun>(true);
        _weapons = new GameObject[guns.Length];

        for (int i = 0; i < guns.Length; i++)
            _weapons[i] = guns[i].gameObject;
    }

    private bool HasWeapon(int weaponIndex)
    {
        return _weapons != null && weaponIndex >= 0 && weaponIndex < _weapons.Length && _weapons[weaponIndex] != null;
    }

    private bool HasOwnedWeapon(int weaponIndex)
    {
        return weaponIndex switch
        {
            PISTOL_ID => _hasPistol,
            RIFLE_ID => _hasRifle,
            SHOTGUN_ID => _hasShotgun,
            _ => false
        };
    }

    private void SetWeaponOwned(int weaponIndex, bool isOwned)
    {
        switch (weaponIndex)
        {
            case PISTOL_ID:
                _hasPistol = isOwned;
                break;
            case RIFLE_ID:
                _hasRifle = isOwned;
                break;
            case SHOTGUN_ID:
                _hasShotgun = isOwned;
                break;
        }
    }

    private void HideAllWeapons()
    {
        if (_weapons == null)
            return;

        for (int i = 0; i < _weapons.Length; i++)
        {
            GameObject weapon = _weapons[i];
            if (weapon != null)
                weapon.SetActive(ShouldKeepWeaponVisibleInWorld(i));
        }
    }

    private bool ShouldKeepWeaponVisibleInWorld(int weaponIndex)
    {
        if (HasOwnedWeapon(weaponIndex) || !HasWeapon(weaponIndex))
            return false;

        return !_weapons[weaponIndex].transform.IsChildOf(transform);
    }

    private void SetFistsActive(bool isActive)
    {
        if (_fists != null)
            _fists.SetEquipped(isActive);

        if (_fistsView != null)
            _fistsView.SetActive(isActive);
    }

    private void EnsureWeaponSlot(int weaponIndex)
    {
        if (_weapons != null && _weapons.Length > weaponIndex)
            return;

        GameObject[] newWeapons = new GameObject[weaponIndex + 1];
        if (_weapons != null)
        {
            for (int i = 0; i < _weapons.Length; i++)
                newWeapons[i] = _weapons[i];
        }

        _weapons = newWeapons;
    }

    private void EnsureEquippedWeaponUsesHolder(ItemWeapons selection, Transform weaponTransform)
    {
        ResolveWeaponHolder();

        if (_weaponHolder == null || weaponTransform == null || weaponTransform.parent == _weaponHolder)
            return;

        if (!weaponTransform.IsChildOf(transform))
            return;

        if (selection == ItemWeapons.PistolClip)
        {
            AttachWeaponToHolder(
                weaponTransform,
                _applyHeldWeaponPoseOnPickup,
                _heldWeaponLocalPosition,
                _heldWeaponLocalEulerAngles,
                _heldWeaponLocalScale);
            return;
        }

        weaponTransform.SetParent(_weaponHolder, false);
    }

    private void FitHeldWeaponIfNeeded(ItemWeapons selection, Transform weaponTransform)
    {
        if (selection != ItemWeapons.ShotgunShell)
            return;

        FitHeldWeaponToView(weaponTransform, new Vector3(0.6f, -0.52f, 1.1f), 1.9f);
    }

    private void FitHeldWeaponToView(Transform weaponTransform, Vector3 desiredLocalCenter, float desiredMaxSize)
    {
        ResolveWeaponHolder();

        if (_weaponHolder == null || weaponTransform == null)
            return;

        if (!TryGetRendererBounds(weaponTransform, out Bounds bounds))
        {
            Debug.LogWarning($"No se encontraron Renderers visibles para encuadrar {weaponTransform.name}.", weaponTransform);
            return;
        }

        float currentMaxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        if (currentMaxSize <= 0f)
            return;

        float scaleFactor = Mathf.Clamp(desiredMaxSize / currentMaxSize, 0.02f, 500f);
        weaponTransform.localScale *= scaleFactor;

        if (!TryGetRendererBounds(weaponTransform, out bounds))
            return;

        Vector3 currentLocalCenter = _weaponHolder.InverseTransformPoint(bounds.center);
        weaponTransform.localPosition += desiredLocalCenter - currentLocalCenter;
    }

    private bool TryGetRendererBounds(Transform root, out Bounds bounds)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        bounds = new Bounds(root.position, Vector3.zero);
        bool hasBounds = false;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
                continue;

            renderer.enabled = true;

            if (!renderer.gameObject.activeInHierarchy)
                continue;

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return hasBounds;
    }

    private void AttachWeaponToHolder(Transform weaponTransform, bool applyHeldPose, Vector3 heldPosition, Vector3 heldEulerAngles, Vector3 heldScale)
    {
        ResolveWeaponHolder();

        if (_weaponHolder == null)
            return;

        weaponTransform.SetParent(_weaponHolder, false);

        if (!applyHeldPose)
            return;

        weaponTransform.localPosition = heldPosition;
        weaponTransform.localEulerAngles = heldEulerAngles;
        weaponTransform.localScale = heldScale;
    }
}
