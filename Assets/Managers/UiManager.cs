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

        if (ActionsManager.instance != null)
            ActionsManager.instance.ReplayUiFeedback();
    }

    private void OnDestroy()
    {
        if (ActionsManager.instance == null)
            return;

        ActionsManager.instance.OnLifeFeedback -= OnLifeFeedback;
        ActionsManager.instance.OnWeaponChangeFeedback -= OnWeaponChangeFeedback;
        ActionsManager.instance.OnWeaponAmmoFeedback -= OnWeaponAmmoFeedback;
        ActionsManager.instance.OnWeaponReloadFeedback -= OnWeaponReloadFeedback;
    }

    #region UI_LIFE_FEEDBACK
    [Header("Life Feedback")]
    [SerializeField] private Image _lifeBar;
    [SerializeField] private Text _lifeValue;
    [SerializeField] private int _life;

    private void LifeSuscription()
    {
        if (ActionsManager.instance == null)
            return;

        ActionsManager.instance.OnLifeFeedback += OnLifeFeedback;
    }

    private void OnLifeFeedback(int currentLife, int maxLife)
    {
        Debug.Log($"valor: {currentLife} - valor max: {maxLife}");
        _life = currentLife;

        float result = CalculatePorcentage(currentLife, maxLife);

        if (_lifeBar != null)
            _lifeBar.fillAmount = result;

        if (_lifeValue != null)
            _lifeValue.text = $"{Mathf.RoundToInt(result * 100f)} %";
    }

    private float CalculatePorcentage(int value, int maxValue)
    {
        if (maxValue <= 0)
            return 0f;

        return Mathf.Clamp01((float)value / (float)maxValue);
    }
    #endregion

    #region UI_WEAPONS_FEEDBACK
    [Header("Currency Feedback")]
    [SerializeField] private List<Sprite> _weaponSprites;
    [SerializeField] private Image _weapon;
    [SerializeField] private Text _ammo;
    [SerializeField] private Text _reloadText;

    private void WeaponsSuscription()
    {
        OnWeaponReloadFeedback(false);

        if (ActionsManager.instance == null)
            return;

        ActionsManager.instance.OnWeaponChangeFeedback += OnWeaponChangeFeedback;
        ActionsManager.instance.OnWeaponAmmoFeedback += OnWeaponAmmoFeedback;
        ActionsManager.instance.OnWeaponReloadFeedback += OnWeaponReloadFeedback;
    }

    public void OnWeaponChangeFeedback(ItemWeapons value)
    {
        int weaponIndex = (int)value;
        if (_weapon == null || _weaponSprites == null || weaponIndex < 0 || weaponIndex >= _weaponSprites.Count)
            return;

        _weapon.sprite = _weaponSprites[weaponIndex];
    }

    public void OnWeaponAmmoFeedback(string value)
    {
        if (_ammo == null)
            return;

        _ammo.text = value;
    }

    public void OnWeaponReloadFeedback(bool isReloading)
    {
        if (_reloadText == null)
            return;

        _reloadText.gameObject.SetActive(isReloading);
    }
    #endregion
}
