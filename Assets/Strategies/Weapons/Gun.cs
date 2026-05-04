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
    private const float DEFAULT_RELOAD_DURATION = 1.5f;

    [SerializeField] private WeaponStats _stats;

    public Transform ParentTransform => _parent;
    [SerializeField] private Transform _parent;
    [SerializeField] protected int _bulletCount;
    [SerializeField] private AudioSource _audioSource;
    private bool _isReloading;

    public GameObject BulletPrefab => _stats != null ? _stats.BulletPrefab : null;
    public int Damage => _stats != null ? _stats.Damage : 0;
    public int ClipSize => _stats != null ? _stats.ClipSize : 0;
    public int BulletsPerShot => _stats != null ? _stats.BulletsPerShot : 0;
    protected virtual float ReloadDuration => DEFAULT_RELOAD_DURATION;
    protected bool CanShoot => !_isReloading && _bulletCount > 0;

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

        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();

        _bulletCount = ClipSize;
        AmmoUiFeedback();
    }

    private void OnDisable()
    {
        if (!_isReloading)
            return;

        _isReloading = false;
        ReloadUiFeedback(false);
    }

    // Instanciar o crear una bala.
    public virtual void Attack()
    {
        PlayShotSound();
        AmmoUiFeedback();
    }

    public void Reload()
    {
        if (_isReloading)
            return;

        Debug.Log($"Recargando {gameObject.name}...");
        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        _isReloading = true;
        ReloadUiFeedback(true);
        PlayReloadSound();

        yield return new WaitForSeconds(ReloadDuration);

        _bulletCount = ClipSize;
        AmmoUiFeedback();
        ReloadUiFeedback(false);
        _isReloading = false;
    }

    protected void AmmoUiFeedback()
    {
        if (ActionsManager.instance != null)
            ActionsManager.instance.ActionWeaponAmmoFeedback($"{_bulletCount}  {ClipSize}");
    }

    private void ReloadUiFeedback(bool isReloading)
    {
        if (ActionsManager.instance != null)
            ActionsManager.instance.ActionWeaponReloadFeedback(isReloading);
    }

    private void PlayShotSound()
    {
        if (_audioSource == null || _stats == null || _stats.ShotSound == null)
            return;

        _audioSource.PlayOneShot(_stats.ShotSound);
    }

    private void PlayReloadSound()
    {
        if (_audioSource == null || _stats == null || _stats.ReloadSound == null)
            return;

        _audioSource.PlayOneShot(_stats.ReloadSound);
    }

    private void AssignDefaultStats()
    {
#if UNITY_EDITOR
        if (_stats == null)
            _stats = AssetDatabase.LoadAssetAtPath<WeaponStats>(DEFAULT_STATS_PATH);
#endif
    }
}
