// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class Character : MonoBehaviour
// {
//     private MovementController _movementController;
//     [SerializeField] private Gun _gun;

//     // BINDING ATTACK KEYS
//     [SerializeField] private KeyCode _shoot = KeyCode.Mouse0;
//     [SerializeField] private KeyCode _reload = KeyCode.R;
//     // BINDING MOVEMENT KEYS
//     [SerializeField] private KeyCode _moveForward = KeyCode.W;
//     [SerializeField] private KeyCode _moveBack = KeyCode.S;
//     [SerializeField] private KeyCode _MoveLeft = KeyCode.A;
//     [SerializeField] private KeyCode _moveRight = KeyCode.D;

//     // SWITCH WEAPONS
//     [SerializeField] private KeyCode _Pistol = KeyCode.Alpha1;
//     [SerializeField] private KeyCode _machingun = KeyCode.Alpha2;
//     [SerializeField] private KeyCode _shotgun = KeyCode.Alpha3;
//     [SerializeField] private List<Gun> _weapons;

//     #region COMMANDS
//     private CmdMovement _cmdMovementForward;
//     private CmdMovement _cmdMovementBack;
//     private CmdMovement _cmdMovementLeft;
//     private CmdMovement _cmdMovementRight;

//     private CmdAttack _cmdAttack;
//     private CmdReload _cmdReload;

//     private CmdApplyDamage _cmdApplyDamage;
//     #endregion

//     void Start()
//     {
//         _movementController = GetComponent<MovementController>();

//         _cmdMovementForward = new CmdMovement(_movementController, transform.forward);
//         _cmdMovementBack = new CmdMovement(_movementController, -transform.forward);
//         _cmdMovementLeft = new CmdMovement(_movementController, -transform.right);
//         _cmdMovementRight = new CmdMovement(_movementController, transform.right);

//         _cmdApplyDamage = new CmdApplyDamage(GetComponent<IDamagable>(), 5);

//         ChangeWeapon(0);
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         if (Input.GetKey(_moveForward)) EventQueueManager.Instance.AddEvent(_cmdMovementForward);
//         if (Input.GetKey(_moveBack)) EventQueueManager.Instance.AddEvent(_cmdMovementBack);
//         if (Input.GetKey(_moveRight)) EventQueueManager.Instance.AddEvent(_cmdMovementRight);
//         if (Input.GetKey(_MoveLeft)) EventQueueManager.Instance.AddEvent(_cmdMovementLeft);

//         if (Input.GetKey(_shoot)) EventQueueManager.Instance.AddEventToQueue(_cmdAttack);
//         if (Input.GetKeyDown(_reload)) EventQueueManager.Instance.AddEventToQueue(_cmdReload);

//         /* Gameover Test */
//         //if (Input.GetKeyDown(KeyCode.Return)) EventManager.instance.EventGameOver(true);

//         /* Lifebar Test */
//         if (Input.GetKeyDown(KeyCode.Backspace)) EventQueueManager.Instance.AddEventToQueue(_cmdApplyDamage);

//         if (Input.GetKeyDown(_Pistol)) ChangeWeapon(0);
//         if (Input.GetKeyDown(_machingun)) ChangeWeapon(1);
//         if (Input.GetKeyDown(_shotgun)) ChangeWeapon(2);
//     }

//     private void ChangeWeapon(int index)
//     {
//         foreach (var gun in _weapons) gun.gameObject.SetActive(false);
//         _weapons[index].gameObject.SetActive(true);
//         EventManager.instance.WeaponChange(index);

//         _gun = _weapons[index];
//         _gun.UI_Updater();
//         _cmdAttack = new CmdAttack(_gun);
//         _cmdReload = new CmdReload(_gun);
//     }
// }


// Usa el nuevo Input System de Unity

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterInputManager : MonoBehaviour
{
    // MOVEMENT STRATEGIES
    private Walk _walk;
    private Run _run;
    private Turn _turn;
    private MovementController _movementController;

    [SerializeField] private Gun _gun;

    // SWITCH WEAPONS
    [SerializeField] private List<Gun> _weapons;

    #region COMMANDS

    private CmdMove _cmdMovementForward;
    private CmdMove _cmdMovementBack;
    private CmdMove _cmdMovementLeft;
    private CmdMove _cmdMovementRight;

    private CmdAttack _cmdAttack;
    private CmdReload _cmdReload;

    private CmdApplyDamage _cmdApplyDamage;

    #endregion

    private void Start()
    {
        _walk = GetComponent<Walk>();
        _run = GetComponent<Run>();
        _turn = GetComponent<Turn>();

        _movementController = GetComponent<MovementController>();

        if (_movementController == null)
        {
            Debug.LogError($"Falta MovementController en {gameObject.name}.");
            enabled = false;
            return;
        }

        if (EventQueueManager.Instance == null)
        {
            Debug.LogError("No hay EventQueueManager en la escena.");
            enabled = false;
            return;
        }

        _cmdMovementForward = new CmdMove(_movementController, transform.forward);
        _cmdMovementBack = new CmdMove(_movementController, -transform.forward);
        _cmdMovementLeft = new CmdMove(_movementController, -transform.right);
        _cmdMovementRight = new CmdMove(_movementController, transform.right);

        _cmdApplyDamage = new CmdApplyDamage(GetComponent<IDamagable>(), 5);

        if (_weapons != null && _weapons.Count > 0)
        {
            ChangeWeapon(0);
        }
        else
        {
            Debug.LogWarning($"No hay armas asignadas en {gameObject.name}.");
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (keyboard == null)
            return;

        // MOVEMENT
        if (keyboard.wKey.isPressed)
            EventQueueManager.Instance.AddEvent(_cmdMovementForward);

        if (keyboard.sKey.isPressed)
            EventQueueManager.Instance.AddEvent(_cmdMovementBack);

        if (keyboard.dKey.isPressed)
            EventQueueManager.Instance.AddEvent(_cmdMovementRight);

        if (keyboard.aKey.isPressed)
            EventQueueManager.Instance.AddEvent(_cmdMovementLeft);

        // ATTACK
        if (mouse != null && mouse.leftButton.isPressed && _cmdAttack != null)
            EventQueueManager.Instance.AddEventToQueue(_cmdAttack);

        // RELOAD
        if (keyboard.rKey.wasPressedThisFrame && _cmdReload != null)
            EventQueueManager.Instance.AddEventToQueue(_cmdReload);

        // LIFEBAR TEST
        if (keyboard.backspaceKey.wasPressedThisFrame && _cmdApplyDamage != null)
            EventQueueManager.Instance.AddEventToQueue(_cmdApplyDamage);

        // SWITCH WEAPONS
        if (keyboard.digit1Key.wasPressedThisFrame)
            ChangeWeapon(0);

        if (keyboard.digit2Key.wasPressedThisFrame)
            ChangeWeapon(1);

        if (keyboard.digit3Key.wasPressedThisFrame)
            ChangeWeapon(2);
    }

    private void ChangeWeapon(int index)
    {
        if (_weapons == null || _weapons.Count == 0)
        {
            Debug.LogWarning("No hay armas asignadas.");
            return;
        }

        if (index < 0 || index >= _weapons.Count)
        {
            Debug.LogWarning($"Índice de arma inválido: {index}.");
            return;
        }

        foreach (Gun gun in _weapons)
        {
            if (gun != null)
                gun.gameObject.SetActive(false);
        }

        _weapons[index].gameObject.SetActive(true);

        if (EventManager.instance != null)
            EventManager.instance.WeaponChange(index);

        _gun = _weapons[index];
        _gun.UI_Updater();

        _cmdAttack = new CmdAttack(_gun);
        _cmdReload = new CmdReload(_gun);
    }
}