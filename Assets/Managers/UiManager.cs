using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using static InventoryManager;

public class UiManager : MonoBehaviour
{
    private void Start()
    {
        LifeSuscription();
        WeaponsSuscription();
    }

    #region UI_LIFE_FEEDBACK
    [Header("Life Feedback")]
    [SerializeField] private Image _lifeBar;
    [SerializeField] private Text _lifeValue;
    [SerializeField] private int _life;

    private void LifeSuscription()
    {
        ActionsManager.instance.OnLifeFeedback += OnLifeFeedback;
    }

    private void OnLifeFeedback(int currentLife, int maxLife)
    {
        Debug.Log($"valor: {currentLife} - valor max: {maxLife}");
        _life = currentLife;

        float result = CalculatePorcentage(currentLife, maxLife);

        _lifeBar.fillAmount = result;
        _lifeValue.text = $"{result * 100} %";

        Color color = result > .5f ? Color.green
                                : result > .2f && result <= .5f ? Color.yellow
                                : Color.red;
        _lifeBar.color = color;
        _lifeValue.color = color;
    }

    private float CalculatePorcentage(int value, int maxValue)
        => (float)value / (float)maxValue;
    #endregion

    #region UI_WEAPONS_FEEDBACK
    [Header("Currency Feedback")]
    [SerializeField] private List<Sprite> _weaponSprites;
    [SerializeField] private Image _weapon;
    [SerializeField] private Text _ammo;

    private void WeaponsSuscription()
    {
        ActionsManager.instance.OnWeaponChangeFeedback += OnWeaponChangeFeedback;
        ActionsManager.instance.OnWeaponAmmoFeedback += OnWeaponAmmoFeedback;
    }

    public void OnWeaponChangeFeedback(ItemWeapons value) => _weapon.sprite = _weaponSprites[(int)value];
    public void OnWeaponAmmoFeedback(string value) => _ammo.text = value;
    #endregion
}
