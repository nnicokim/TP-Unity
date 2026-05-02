using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class Gun : MonoBehaviour, IGun
{
    private const string DEFAULT_STATS_PATH = "Assets/Flyweight/WeaponStats.asset";

    [SerializeField] private WeaponStats _stats;

    public Transform ParentTransform => _parent;
    [SerializeField] private Transform _parent;
    [SerializeField] protected int _bulletCount;

    public GameObject BulletPrefab => _stats != null ? _stats.BulletPrefab : null;
    public int Damage => _stats != null ? _stats.Damage : 0;
    public int ClipSize => _stats != null ? _stats.ClipSize : 0;
    public int BulletsPerShot => _stats != null ? _stats.BulletsPerShot : 0;

    private void Reset()
    {
        AssignDefaultStats();
    }

    private void OnValidate()
    {
        AssignDefaultStats();
    }

    private void Start()
    {
        AssignDefaultStats();

        if (_stats == null)
        {
            Debug.LogError($"Faltan WeaponStats en {gameObject.name}.", this);
            enabled = false;
            return;
        }

        Reload();
    }

    // Instanciar o crear una bala.
    public virtual void Attack()
    {
        AmmoUiFeedback();
    }

    public void Reload()
    {
        _bulletCount = ClipSize;
        AmmoUiFeedback();
    }

    protected void AmmoUiFeedback()
    {
        if (ActionsManager.instance != null)
            ActionsManager.instance.ActionWeaponAmmoFeedback($"{_bulletCount} / {ClipSize}");
    }

    private void AssignDefaultStats()
    {
#if UNITY_EDITOR
        if (_stats == null)
            _stats = AssetDatabase.LoadAssetAtPath<WeaponStats>(DEFAULT_STATS_PATH);
#endif
    }
}
