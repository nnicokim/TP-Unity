using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    /* Image References */
    [SerializeField] private Image _weapon;
    [SerializeField] private Image _lifebar;
    [SerializeField] private Image _avatar;

    [SerializeField] private List<Sprite> _avatarFaces;
    [SerializeField] private List<Sprite> _weapons;

    /* Text References */
    [SerializeField] private Text _ammoValue;

    /* Variables */
    private float _characterCurrentLife;

    private void Start()
    {
        /* Suscripcion de eventos */
        // TODO: ver tema avatarChange, quizas para esta entrega no llegamos
        EventManager.instance.OnAmmoChange += UpdateAmmoValue;
        EventManager.instance.OnCharacterLifeChange += UpdateLifebar;
        EventManager.instance.OnAvatarChange += UpdateFightingAvatarSprite;
        EventManager.instance.OnWeaponChange += UpdateWeapon;
    }

    private void UpdateFightingAvatarSprite()
    {
        _avatar.sprite = _avatarFaces[2];
        Invoke("UpdateNormalAvatarSprite", 1f);
    }

    private void UpdateLifebar(float currentLife, float maxLife)
    {
        _lifebar.fillAmount = currentLife / maxLife;
        _characterCurrentLife = currentLife;
    }

    private void UpdateAmmoValue(int currentAmmo, int maxAmmo)
    {
        _ammoValue.text = $"{currentAmmo} de {maxAmmo}";
    }

    private void UpdateWeapon(int weaponIndex)
    {
        _weapon.sprite = _weapons[weaponIndex];
    }

    private void UpdateNormalAvatarSprite()
    {
        _avatar.sprite = _characterCurrentLife <= 20 ? _avatarFaces[0] : _avatarFaces[1];
    }
}

/*
 Notas de pendientes
 - clase est�tica Utils con constantes y enums para listas
 - pasar sprite de armas a blanco
 - armar los eventos
 - evaluar de agregar sonidos de disparo y pickup coins
 - x
 */
