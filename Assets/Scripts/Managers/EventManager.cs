using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    static public EventManager instance;

    #region UNITY_EVENTS
    private void Awake()
    {
        if (instance != null) Destroy(this);
        instance = this;
    }
    #endregion

    #region GAME_MANAGER 
    // TODO: falta esto de gameover
    // public event Action<bool> OnGameOver;

    // public void EventGameOver(bool isVictory)
    // {
    //     if (OnGameOver != null) OnGameOver(isVictory);
    // }
    #endregion

    #region IN_GAME_UI
    public event Action<int, int> OnAmmoChange;
    public event Action<float, float> OnCharacterLifeChange;
    public event Action<int> OnWeaponChange;
    public event Action<int> OnEnemiesKilledChange;
    public event Action<float> OnTimerChange;

    public void AmmoChange(int currentAmmo, int maxAmmo)
    {
        if (OnAmmoChange != null) OnAmmoChange(currentAmmo, maxAmmo);
    }

    public void CharacterLifeChange(float currentLife, float maxLife)
    {
        if (OnCharacterLifeChange != null) OnCharacterLifeChange(currentLife, maxLife);
    }

    public void WeaponChange(int weaponIndex)
    {
        if (OnWeaponChange != null) OnWeaponChange(weaponIndex);
    }

    public void EnemiesKilledChange(int enemiesKilled)
    {
        // TODO: implementar
    }

    public void TimerChange(float time)
    {
        // TODO: implementar
    }
    #endregion
}