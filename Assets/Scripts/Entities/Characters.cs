using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    private MovementController _movementController;
    [SerializeField] private Gun _gun;

    // BINDING ATTACK KEYS
    [SerializeField] private KeyCode _shoot = KeyCode.Mouse0;
    [SerializeField] private KeyCode _reload = KeyCode.R;
    // BINDING MOVEMENT KEYS
    [SerializeField] private KeyCode _moveForward = KeyCode.W;
    [SerializeField] private KeyCode _moveBack = KeyCode.S;
    [SerializeField] private KeyCode _MoveLeft = KeyCode.A;
    [SerializeField] private KeyCode _moveRight = KeyCode.D;

    // SWITCH WEAPONS
    [SerializeField] private KeyCode _Pistol = KeyCode.Alpha1;
    [SerializeField] private KeyCode _machingun = KeyCode.Alpha2;
    [SerializeField] private KeyCode _shotgun = KeyCode.Alpha3;
    [SerializeField] private List<Gun> _weapons;

    #region COMMANDS
    private CmdMovement _cmdMovementForward;
    private CmdMovement _cmdMovementBack;
    private CmdMovement _cmdMovementLeft;
    private CmdMovement _cmdMovementRight;

    private CmdAttack _cmdAttack;
    private CmdReload _cmdReload;

    private CmdApplyDamage _cmdApplyDamage;
    #endregion

    void Start()
    {
        _movementController = GetComponent<MovementController>();

        _cmdMovementForward = new CmdMovement(_movementController, transform.forward);
        _cmdMovementBack = new CmdMovement(_movementController, -transform.forward);
        _cmdMovementLeft = new CmdMovement(_movementController, -transform.right);
        _cmdMovementRight = new CmdMovement(_movementController, transform.right);

        _cmdApplyDamage = new CmdApplyDamage(GetComponent<IDamagable>(), 5);

        ChangeWeapon(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(_moveForward)) EventQueueManager.Instance.AddEvent(_cmdMovementForward);
        if (Input.GetKey(_moveBack)) EventQueueManager.Instance.AddEvent(_cmdMovementBack);
        if (Input.GetKey(_moveRight)) EventQueueManager.Instance.AddEvent(_cmdMovementRight);
        if (Input.GetKey(_MoveLeft)) EventQueueManager.Instance.AddEvent(_cmdMovementLeft);

        if (Input.GetKey(_shoot)) EventQueueManager.Instance.AddEventToQueue(_cmdAttack);
        if (Input.GetKeyDown(_reload)) EventQueueManager.Instance.AddEventToQueue(_cmdReload);

        /* Gameover Test */
        if (Input.GetKeyDown(KeyCode.Return)) EventManager.instance.EventGameOver(true);
        /* Lifebar Test */
        if (Input.GetKeyDown(KeyCode.Backspace)) EventQueueManager.Instance.AddEventToQueue(_cmdApplyDamage);

        if (Input.GetKeyDown(_Pistol)) ChangeWeapon(0);
        if (Input.GetKeyDown(_machingun)) ChangeWeapon(1);
        if (Input.GetKeyDown(_shotgun)) ChangeWeapon(2);
    }

    private void ChangeWeapon(int index)
    {
        foreach (var gun in _weapons) gun.gameObject.SetActive(false);
        _weapons[index].gameObject.SetActive(true);
        EventManager.instance.WeaponChange(index);

        _gun = _weapons[index];
        _gun.UI_Updater();
        _cmdAttack = new CmdAttack(_gun);
        _cmdReload = new CmdReload(_gun);
    }
}
