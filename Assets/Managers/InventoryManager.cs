using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    #region SINGLETON
    static public InventoryManager instance;

    private void Awake()
    {
        if (instance != null) Destroy(instance);
        instance = this;
    }
    #endregion

    #region ITEM_ENUMS

    public enum ItemWeapons
    {
        PistolClip = 0,
        RifleClip = 1,
        ShotgunShell = 2
    }
    #endregion

    private InventoryWeapons _inventoryWeapons;

    #region  UNITY_EVENTS
    private void Start()
    {
        _inventoryWeapons = GetComponent<InventoryWeapons>();

        ItemSuscription();
    }
    #endregion

    #region ACTION_ITEMS
    private void ItemSuscription()
    {
        ActionsManager.instance.OnItemWeaponInteraction += OnItemWeaponInteraction;
    }

    private void OnItemWeaponInteraction(ItemWeapons item) { }
    #endregion
}
