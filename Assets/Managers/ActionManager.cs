using System;
using UnityEngine;
using static InventoryManager;
using static UiManager;

public class ActionsManager : MonoBehaviour
{
    #region SINGLETON
    static public ActionsManager instance;

    private void Awake()
    {
        if (instance != null) Destroy(instance);
        instance = this;
    }
    #endregion

    #region GAMEOVER_ACTIONS
    public event Action<bool> OnGameover;

    public void ActionGameover(bool isVictory) => OnGameover(isVictory);
    #endregion

    #region ITEM_ACTIONS
    public event Action<ItemWeapons> OnItemWeaponInteraction;

    public void ActionWeaponInteraction(ItemWeapons item) => OnItemWeaponInteraction(item);
    #endregion

    #region UI_LIFE_FEEDBACK
    public event Action<int, int> OnLifeFeedback;
    private bool _hasLifeFeedback;
    private int _currentLife;
    private int _maxLife;

    public void ActionLifeFeedback(int value, int maxValue)
    {
        _hasLifeFeedback = true;
        _currentLife = value;
        _maxLife = maxValue;

        if (OnLifeFeedback != null)
            OnLifeFeedback(value, maxValue);
    }
    #endregion

    #region UI_WEAPONS_FEEDBACK
    public event Action<ItemWeapons> OnWeaponChangeFeedback;
    public event Action<string> OnWeaponAmmoFeedback;
    public event Action<bool> OnWeaponReloadFeedback;
    private bool _hasWeaponChangeFeedback;
    private ItemWeapons _currentWeapon;
    private bool _hasWeaponAmmoFeedback;
    private string _weaponAmmoFeedback;
    private bool _hasWeaponReloadFeedback;
    private bool _isWeaponReloading;

    public void ActionWeaponChangeFeedback(ItemWeapons value)
    {
        _hasWeaponChangeFeedback = true;
        _currentWeapon = value;

        if (OnWeaponChangeFeedback != null)
            OnWeaponChangeFeedback(value);
    }
    public void ActionWeaponAmmoFeedback(string value)
    {
        _hasWeaponAmmoFeedback = true;
        _weaponAmmoFeedback = value;

        if (OnWeaponAmmoFeedback != null)
            OnWeaponAmmoFeedback(value);
    }

    public void ActionWeaponReloadFeedback(bool isReloading)
    {
        _hasWeaponReloadFeedback = true;
        _isWeaponReloading = isReloading;

        if (OnWeaponReloadFeedback != null)
            OnWeaponReloadFeedback(isReloading);
    }
    #endregion

    #region UI_REPLAY
    public void ReplayUiFeedback()
    {
        if (_hasLifeFeedback && OnLifeFeedback != null)
            OnLifeFeedback(_currentLife, _maxLife);

        if (_hasWeaponChangeFeedback && OnWeaponChangeFeedback != null)
            OnWeaponChangeFeedback(_currentWeapon);

        if (_hasWeaponAmmoFeedback && OnWeaponAmmoFeedback != null)
            OnWeaponAmmoFeedback(_weaponAmmoFeedback);

        if (_hasWeaponReloadFeedback && OnWeaponReloadFeedback != null)
            OnWeaponReloadFeedback(_isWeaponReloading);
    }
    #endregion
}
