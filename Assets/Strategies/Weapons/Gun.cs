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
    private const string FULL_AMMO_RELOAD_MESSAGE = "Amunicion completa. No se puede recargar";

    [SerializeField] private WeaponStats _stats;

    public Transform ParentTransform => _parent;
    [SerializeField] private Transform _parent;
    [SerializeField] protected int _bulletCount;
    [SerializeField] private AudioSource _audioSource;

    [Header("Camera Aim")]
    [SerializeField] private Camera _aimCamera;
    [SerializeField] private float _aimDistance = 1000f;
    [SerializeField] private LayerMask _aimMask = ~0;

    private bool _isInitialized;
    private bool _isReloading;

    public GameObject BulletPrefab => _stats != null ? _stats.BulletPrefab : null;
    public int Damage => _stats != null ? _stats.Damage : 0;
    public int ClipSize => _stats != null ? _stats.ClipSize : 0;
    public int BulletsPerShot => _stats != null ? _stats.BulletsPerShot : 0;
    public float BulletMaxPositionRadius => _stats != null ? _stats.BulletMaxPositionRadius : 0;
    public float BulletMaxRandomAngle => _stats != null ?  _stats.BulletMaxRandomAngle : 0;
    protected virtual float ReloadDuration => _stats != null ? _stats.BulletReloadTime : DEFAULT_RELOAD_DURATION;
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
        InitializeGun();
    }

    private void InitializeGun()
    {
        if (_isInitialized)
            return;

        AssignDefaultStats();

        if (_stats == null)
        {
            Debug.LogError($"Faltan WeaponStats en {gameObject.name}.", this);
            enabled = false;
            return;
        }

        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();

        ResolveAimCamera();

        _bulletCount = ClipSize;
        _isInitialized = true;
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
    protected void CreateBullet(Vector3 position, Quaternion rotation)
    {
        if (CanShoot)
        {
            GameObject bullet = Instantiate(BulletPrefab, position, rotation, ParentTransform);

            IBullet bulletBehaviour = bullet.GetComponent<IBullet>();
            if (bulletBehaviour == null)
            {
                Debug.LogError($"El prefab {BulletPrefab.name} no tiene un componente IBullet.", bullet);
                Destroy(bullet);
                return;
            }

            bulletBehaviour.SetOwner(this);
            bullet.name = "Bullet";
        }
    }

    protected void CreateSingleBullet()
    {
        CreateBullet(transform.position, GetShootRotation(transform.position));
    }
    protected void CreateRandomBullets()
    {
        for (int i = 0; i < BulletsPerShot; i++)
        {
            Vector3 spawnPosition = transform.position + Random.insideUnitSphere * BulletMaxPositionRadius;
            Quaternion shootRotation = GetShootRotation(spawnPosition) * Quaternion.AngleAxis(Random.Range(0, BulletMaxRandomAngle), spawnPosition);
            CreateBullet(spawnPosition, shootRotation);
        }
    }
    public virtual void Attack()
    {
        PlayShotSound();
        AmmoUiFeedback();
        ReloadIfEmpty();
    }

    public void Reload()
    {
        InitializeGun();

        if (_isReloading)
            return;

        if (HasFullAmmo())
        {
            Debug.Log(FULL_AMMO_RELOAD_MESSAGE);
            return;
        }

        Debug.Log($"Recargando {gameObject.name}...");
        StartCoroutine(ReloadRoutine());
    }

    public void RefreshAmmoUi()
    {
        InitializeGun();
        AmmoUiFeedback();
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
        if (ActionsManager.instance != null && _bulletCount >= 10)
            ActionsManager.instance.ActionWeaponAmmoFeedback($"{_bulletCount}   {ClipSize}");
        else if (ActionsManager.instance != null && _bulletCount < 10)
            ActionsManager.instance.ActionWeaponAmmoFeedback($"  {_bulletCount}   {ClipSize}");
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

    private void ReloadIfEmpty()
    {
        if (_bulletCount > 0 || _isReloading)
            return;

        Reload();
    }

    private bool HasFullAmmo()
    {
        return ClipSize > 0 && _bulletCount >= ClipSize;
    }

    protected Quaternion GetShootRotation(Vector3 spawnPosition)
    {
        Vector3 direction = GetShootDirection(spawnPosition);

        if (direction.sqrMagnitude <= 0f)
            return transform.parent != null ? transform.parent.rotation : transform.rotation;

        return Quaternion.LookRotation(direction);
    }

    protected Vector3 GetShootDirection(Vector3 spawnPosition)
    {
        Vector3 aimPoint = GetCameraAimPoint();
        Vector3 direction = aimPoint - spawnPosition;

        if (direction.sqrMagnitude <= 0f)
            return transform.parent != null ? transform.parent.forward : transform.forward;

        return direction.normalized;
    }

    private Vector3 GetCameraAimPoint()
    {
        ResolveAimCamera();

        if (_aimCamera == null)
            return transform.position + (transform.parent != null ? transform.parent.forward : transform.forward) * _aimDistance;

        Ray aimRay = _aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit[] hits = Physics.RaycastAll(aimRay, _aimDistance, _aimMask, QueryTriggerInteraction.Ignore);

        float closestDistance = float.PositiveInfinity;
        Vector3 closestPoint = aimRay.origin + aimRay.direction * _aimDistance;

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            if (hit.collider == null || hit.transform.IsChildOf(transform.root))
                continue;

            if (hit.distance >= closestDistance)
                continue;

            closestDistance = hit.distance;
            closestPoint = hit.point;
        }

        return closestPoint;
    }

    private void ResolveAimCamera()
    {
        if (_aimCamera == null)
            _aimCamera = Camera.main;
    }

    private void AssignDefaultStats()
    {
#if UNITY_EDITOR
        if (_stats == null)
            _stats = AssetDatabase.LoadAssetAtPath<WeaponStats>(DEFAULT_STATS_PATH);
#endif
    }
}
