using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterInputManager : MonoBehaviour
{
    // MOVEMENT STRATEGIES
    private Walk _walk;
    private Run _run;
    private Turn _turn;

    // COMMANDS - MOVEMENT
    private CmdMove _cmdMoveForward;
    private CmdMove _cmdMoveBack;

    private CmdMove _cmdTurnLeft;
    private CmdMove _cmdTurnRight;

    private CmdMove _cmdRunForward;
    private CmdMove _cmdRunBack;

    // WEAPONS LIST
    private const int PISTOL_ID = 0; // TODO: ver como hacer si solo porta 2 armas.
    private const int RIFLE_ID = 1;
    private const int SHOTGUN_ID = 2;

    private enum ItemWeapons
    {
        PistolClip = PISTOL_ID,
        RifleClip = RIFLE_ID,
        ShotgunClip = SHOTGUN_ID
    }

    [SerializeField] private GameObject[] _weapons;
    [SerializeField] private Gun _equipedGun; // main gun strategy

    // COMMANDS - WEAPONS
    private CmdAttack _cmdAttack;
    private CmdReload _cmdReload;

    private void Start()
    {
        // Asignación de referencias
        _walk = GetComponent<Walk>();
        _run = GetComponent<Run>();
        _turn = GetComponent<Turn>();

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

        if (_turn == null)
        {
            Debug.LogError($"Falta el componente Turn en {gameObject.name}.");
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

        if (_equipedGun == null && _weapons != null && _weapons.Length > PISTOL_ID && _weapons[PISTOL_ID] != null)
            _equipedGun = _weapons[PISTOL_ID].GetComponent<Gun>();

        if (_equipedGun == null)
            _equipedGun = GetComponentInChildren<Gun>(true);

        if (_equipedGun == null)
        {
            Debug.LogError($"No hay un arma equipada en {gameObject.name}.");
            enabled = false;
            return;
        }

        // Nueva instancia de comandos - Caminar
        _cmdMoveForward = new CmdMove(_walk, Vector3.forward);
        _cmdMoveBack = new CmdMove(_walk, -Vector3.forward);

        // Nueva instancia de comandos - Correr
        _cmdRunForward = new CmdMove(_run, Vector3.forward);
        _cmdRunBack = new CmdMove(_run, -Vector3.forward);

        // Nueva instancia de comandos - Girar
        _cmdTurnLeft = new CmdMove(_turn, -Vector3.up);
        _cmdTurnRight = new CmdMove(_turn, Vector3.up);

        // Nueva instancia de comandos - Armas
        _cmdAttack = new CmdAttack(_equipedGun);
        _cmdReload = new CmdReload(_equipedGun);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (keyboard == null)
            return;

        bool isRunning = keyboard.leftShiftKey.isPressed;

        // MOVEMENT
        if (keyboard.wKey.isPressed)
        {
            EventQueueManager.instance.AddCommand(isRunning ? _cmdRunForward : _cmdMoveForward);
        }

        if (keyboard.sKey.isPressed)
        {
            EventQueueManager.instance.AddCommand(isRunning ? _cmdRunBack : _cmdMoveBack);
        }

        if (keyboard.aKey.isPressed)
        {
            EventQueueManager.instance.AddCommand(_cmdTurnLeft);
        }

        if (keyboard.dKey.isPressed)
        {
            EventQueueManager.instance.AddCommand(_cmdTurnRight);
        }

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
            WeaponsSelection(ItemWeapons.ShotgunClip);
        }
    }

    private void WeaponsSelection(ItemWeapons selection)
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
        //ActionsManager.instance.ActionWeaponChangeFeedback(selection);

        // 5. Reload weapon
        EventQueueManager.instance.AddCommand(_cmdReload);
    }

    private void LoadWeaponsFromChildren()
    {
        Gun[] guns = GetComponentsInChildren<Gun>(true);
        _weapons = new GameObject[guns.Length];

        for (int i = 0; i < guns.Length; i++)
            _weapons[i] = guns[i].gameObject;
    }
}
