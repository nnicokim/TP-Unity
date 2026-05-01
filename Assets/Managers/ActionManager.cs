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

    public void ActionLifeFeedback(int value, int maxValue) { if (OnLifeFeedback != null) OnLifeFeedback(value, maxValue); }
    #endregion

    #region UI_WEAPONS_FEEDBACK
    public event Action<ItemWeapons> OnWeaponChangeFeedback;
    public event Action<string> OnWeaponAmmoFeedback;

    public void ActionWeaponChangeFeedback(ItemWeapons value) { if (OnWeaponChangeFeedback != null) OnWeaponChangeFeedback(value); }
    public void ActionWeaponAmmoFeedback(string value) { if (OnWeaponAmmoFeedback != null) OnWeaponAmmoFeedback(value); }
    #endregion
}
