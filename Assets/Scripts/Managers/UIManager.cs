using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    /* Image References */
    [SerializeField] private Image _weapon;
    [SerializeField] private Image _lifebar;
    [SerializeField] private List<Sprite> _weapons;

    /* Text References */
    [SerializeField] private Text _weaponName;
    [SerializeField] private Text _ammoValue;
    [SerializeField] private Text _enemiesKilled;
    [SerializeField] private Text _timer;

    /* Variables */
    private float _characterCurrentLife;

    private void Start()
    {
        /* Suscripcion de eventos */
        EventManager.instance.OnAmmoChange += UpdateAmmoValue;
        EventManager.instance.OnCharacterLifeChange += UpdateLifebar;
        EventManager.instance.OnWeaponChange += UpdateWeapon;
        EventManager.instance.OnEnemiesKilledChange += UpdateEnemiesKilled;
        EventManager.instance.OnTimerChange += UpdateTimer;
    }

    private void UpdateLifebar(float currentLife, float maxLife)
    {
        _lifebar.fillAmount = currentLife / maxLife;
        _characterCurrentLife = currentLife;
    }

    private void UpdateAmmoValue(int currentAmmo, int maxAmmo)
    {
        _ammoValue.text = $"{currentAmmo} / {maxAmmo}";
    }

    private void UpdateWeapon(int weaponIndex)
    {
        _weapon.sprite = _weapons[weaponIndex];
        _weaponName.text = _weapons[weaponIndex].name;
    }

    private void UpdateEnemiesKilled(int enemiesKilled)
    {
        _enemiesKilled.text = $"Enemies killed: {enemiesKilled}";
    }

    private void UpdateTimer(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        _timer.text = $"Time in level: {minutes:00}:{seconds:00}";
    }
}