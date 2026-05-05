using UnityEngine;
using UnityEngine.InputSystem;
using static InventoryManager;

public class CharacterInputManager : MonoBehaviour
{
    // MOVEMENT STRATEGIES
    private Walk _walk;
    private Run _run;

    // WEAPONS LIST
    private const int PISTOL_ID = 0; // TODO: ver como hacer si solo porta 2 armas.
    private const int RIFLE_ID = 1;
    private const int SHOTGUN_ID = 2;

    [SerializeField] private GameObject[] _weapons;
    [SerializeField] private Gun _equipedGun; // main gun strategy

    [Header("Camera Relative Movement")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private bool _faceCameraDirection = true;
    [SerializeField] private float _rotationSmoothSpeed = 12f;

    // COMMANDS - WEAPONS
    private CmdAttack _cmdAttack;
    private CmdReload _cmdReload;

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

        if (_weapons == null || _weapons.Length == 0)
            LoadWeaponsFromChildren();

        if (_weapons == null || _weapons.Length == 0)
        {
            Debug.LogError($"No hay armas configuradas en {gameObject.name}.");
            enabled = false;
            return;
        }

        ResolveCameraTransform();
        EquipDefaultWeapon();
    }

    private void EquipDefaultWeapon()
    {
        foreach (var weapon in _weapons)
        {
            if (weapon != null)
                weapon.SetActive(false);
        }

        if (_weapons[PISTOL_ID] == null)
        {
            Debug.LogError($"No hay arma por defecto (pistola) configurada en {gameObject.name}.");
            enabled = false;
            return;
        }

        _weapons[PISTOL_ID].SetActive(true);
        _equipedGun = _weapons[PISTOL_ID].GetComponent<Gun>();

        if (_equipedGun == null)
        {
            Debug.LogError($"{_weapons[PISTOL_ID].name} no tiene componente Gun.");
            enabled = false;
            return;
        }

        _cmdAttack = new CmdAttack(_equipedGun);
        _cmdReload = new CmdReload(_equipedGun);

        if (ActionsManager.instance != null)
            ActionsManager.instance.ActionWeaponChangeFeedback(ItemWeapons.PistolClip);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (keyboard == null)
            return;

        bool isRunning = keyboard.leftShiftKey.isPressed;

        HandleMovementInput(keyboard, isRunning);
        RotateTowardsCameraDirection();

        // WEAPONS
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            EventQueueManager.instance.AddCommand(_cmdAttack);
        }

        if (keyboard.rKey.wasPressedThisFrame)
        {
            EventQueueManager.instance.AddCommand(_cmdReload);
        }

        // WEAPONS SELECTION
        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            WeaponsSelection(ItemWeapons.PistolClip);
        }

        if (keyboard.digit2Key.wasPressedThisFrame)
        {
            WeaponsSelection(ItemWeapons.RifleClip);
        }

        if (keyboard.digit3Key.wasPressedThisFrame)
        {
            WeaponsSelection(ItemWeapons.ShotgunShell);
        }
    }

    private void WeaponsSelection(InventoryManager.ItemWeapons selection)
    {
        if (_weapons == null || _weapons.Length == 0)
            LoadWeaponsFromChildren();

        int weaponIndex = (int)selection;
        if (_weapons == null || weaponIndex >= _weapons.Length || _weapons[weaponIndex] == null)
        {
            Debug.LogWarning($"No hay arma configurada para {selection}.");
            return;
        }

        // 1. Desactivado de todas las armas
        foreach (var weapon in _weapons)
        {
            if (weapon != null)
                weapon.SetActive(false);
        }

        // 2. Activar el arma seleccionada
        _equipedGun = _weapons[weaponIndex].GetComponent<Gun>();
        if (_equipedGun == null)
        {
            Debug.LogWarning($"{_weapons[weaponIndex].name} no tiene componente Gun.");
            return;
        }

        _equipedGun.gameObject.SetActive(true);

        // 3. Crear nuevas las estrategias
        _cmdAttack = new CmdAttack(_equipedGun);
        _cmdReload = new CmdReload(_equipedGun);

        // 4. Update Ui Feedback
        ActionsManager.instance.ActionWeaponChangeFeedback(selection);

        // 5. Reload weapon
        EventQueueManager.instance.AddCommand(_cmdReload);
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

        if (keyboard.wKey.isPressed)
            direction += forward;

        if (keyboard.sKey.isPressed)
            direction -= forward;

        if (keyboard.aKey.isPressed)
            direction -= right;

        if (keyboard.dKey.isPressed)
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

    private void LoadWeaponsFromChildren()
    {
        Gun[] guns = GetComponentsInChildren<Gun>(true);
        _weapons = new GameObject[guns.Length];

        for (int i = 0; i < guns.Length; i++)
            _weapons[i] = guns[i].gameObject;
    }
}
