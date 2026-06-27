using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Serialization;

public class Gun : MonoBehaviour, IGun
{
    private const string DEFAULT_STATS_PATH = "Assets/Flyweight/WeaponStats.asset";
    private const float DEFAULT_RELOAD_DURATION = 1.5f;
    private const string FULL_AMMO_RELOAD_MESSAGE = "Amunicion completa. No se puede recargar";

    [SerializeField] private WeaponStats _stats;

    public Transform ParentTransform => _parent;
    [SerializeField] private Transform _parent;
    [SerializeField] private AudioSource _audioSource;

    [Header("Ammo")]
    [SerializeField, Min(1)] private int magazineSize;
    [SerializeField, Min(0), FormerlySerializedAs("_bulletCount")] protected int currentAmmo;
    [SerializeField, Min(0)] private int reserveAmmo;
    [SerializeField] private bool initializeAmmoOnStart = true;
    [SerializeField] private bool autoReloadWhenEmpty;

    [Header("Audio Overrides")]
    [SerializeField] private AudioClip _shotSoundOverride;
    [SerializeField] private AudioClip _reloadSoundOverride;

    [Header("Shoot Origin")]
    [SerializeField] private Transform _muzzleTransform;

    [Header("Fire Rate")]
    [SerializeField, Min(0f)] private float _secondsBetweenShots;

    [Header("Visual Recoil")]
    [SerializeField] private WeaponRecoil _weaponRecoil;

    [Header("Camera Aim")]
    [SerializeField] private Camera _aimCamera;
    [SerializeField] private float _aimDistance = 1000f;
    [SerializeField] private LayerMask _aimMask = ~0;

    private bool _isInitialized;
    private bool _isReloading;
    private float _nextShootTime;
    private bool _warnedMissingAudioSource;
    private bool _warnedMissingShotSound;
    private bool _warnedMissingReloadSound;

    public GameObject BulletPrefab => _stats != null ? _stats.BulletPrefab : null;
    public int Damage => _stats != null ? _stats.Damage : 0;
    public int ClipSize => MagazineSize;
    public int BulletsPerShot => _stats != null ? _stats.BulletsPerShot : 0;
    public float BulletMaxPositionRadius => _stats != null ? _stats.BulletMaxPositionRadius : 0;
    public float BulletMaxRandomAngle => _stats != null ?  _stats.BulletMaxRandomAngle : 0;
    private AudioClip ShotSound => _shotSoundOverride != null ? _shotSoundOverride : _stats != null ? _stats.ShotSound : null;
    private AudioClip ReloadSound => _reloadSoundOverride != null ? _reloadSoundOverride : _stats != null ? _stats.ReloadSound : null;
    protected virtual float ReloadDuration => _stats != null ? _stats.BulletReloadTime : DEFAULT_RELOAD_DURATION;
    protected bool CanShoot => !_isReloading && currentAmmo > 0 && Time.time >= _nextShootTime;
    protected Vector3 MuzzlePosition => _muzzleTransform != null ? _muzzleTransform.position : transform.position;
    protected Transform MuzzleTransform => _muzzleTransform;
    private int MagazineSize => magazineSize > 0 ? magazineSize : _stats != null ? _stats.ClipSize : 0;

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

        if (_weaponRecoil == null)
            _weaponRecoil = GetComponent<WeaponRecoil>();

        ResolveAimCamera();
        InitializeAmmo();

        _isInitialized = true;
        AmmoUiFeedback();
    }

    private void InitializeAmmo()
    {
        if (magazineSize <= 0 && _stats != null)
            magazineSize = Mathf.Max(1, _stats.ClipSize);

        if (!initializeAmmoOnStart)
        {
            currentAmmo = Mathf.Clamp(currentAmmo, 0, MagazineSize);
            reserveAmmo = Mathf.Max(0, reserveAmmo);
            return;
        }

        currentAmmo = MagazineSize;
        reserveAmmo = MagazineSize * 4;
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
        Vector3 spawnPosition = MuzzlePosition;
        CreateBullet(spawnPosition, GetShootRotation(spawnPosition));
    }
    protected void CreateRandomBullets()
    {
        for (int i = 0; i < BulletsPerShot; i++)
        {
            Vector3 spawnPosition = MuzzlePosition + Random.insideUnitSphere * BulletMaxPositionRadius;
            Quaternion shootRotation = GetShootRotation(spawnPosition) * Quaternion.AngleAxis(Random.Range(0, BulletMaxRandomAngle), spawnPosition);
            CreateBullet(spawnPosition, shootRotation);
        }
    }
    public virtual void Attack()
    {
        ApplyVisualRecoil();
        RegisterShotCooldown();
        PlayShotSound();
        AmmoUiFeedback();

        if (autoReloadWhenEmpty)
            ReloadIfEmpty();
    }

    public void ResetVisualRecoilRestPose()
    {
        if (_weaponRecoil == null)
            _weaponRecoil = GetComponent<WeaponRecoil>();

        if (_weaponRecoil != null)
            _weaponRecoil.ResetRestPose();
    }

    private void ApplyVisualRecoil()
    {
        if (_weaponRecoil == null)
            _weaponRecoil = GetComponent<WeaponRecoil>();

        if (_weaponRecoil != null)
            _weaponRecoil.ApplyRecoil();
    }

    protected void RegisterShotCooldown()
    {
        if (_secondsBetweenShots <= 0f)
            return;

        _nextShootTime = Time.time + _secondsBetweenShots;
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

        if (reserveAmmo <= 0)
        {
            Debug.Log($"Sin municion de reserva para {gameObject.name}.");
            AmmoUiFeedback();
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

    public void PlayReloadSoundOnce()
    {
        AssignDefaultStats();

        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();

        PlayReloadSound();
    }

    private IEnumerator ReloadRoutine()
    {
        _isReloading = true;
        ReloadUiFeedback(true);
        PlayReloadSound();

        yield return new WaitForSeconds(ReloadDuration);

        int missingAmmo = MagazineSize - currentAmmo;
        int ammoToLoad = Mathf.Min(missingAmmo, reserveAmmo);

        currentAmmo += ammoToLoad;
        reserveAmmo -= ammoToLoad;
        currentAmmo = Mathf.Clamp(currentAmmo, 0, MagazineSize);
        reserveAmmo = Mathf.Max(0, reserveAmmo);

        AmmoUiFeedback();
        ReloadUiFeedback(false);
        _isReloading = false;
    }

    protected void AmmoUiFeedback()
    {
        if (ActionsManager.instance != null)
            ActionsManager.instance.ActionWeaponAmmoFeedback($"{currentAmmo}   {reserveAmmo}");
    }

    private void ReloadUiFeedback(bool isReloading)
    {
        if (ActionsManager.instance != null)
            ActionsManager.instance.ActionWeaponReloadFeedback(isReloading);
    }

    private void PlayShotSound()
    {
        if (_audioSource == null)
        {
            WarnOnce(ref _warnedMissingAudioSource, $"Falta AudioSource en {gameObject.name}.", this);
            return;
        }

        if (ShotSound == null)
        {
            WarnOnce(ref _warnedMissingShotSound, $"Falta sonido de disparo en {gameObject.name}. Asigna Shot Sound Override o WeaponStats.ShotSound.", this);
            return;
        }

        _audioSource.PlayOneShot(ShotSound);
    }

    private void PlayReloadSound()
    {
        if (_audioSource == null)
        {
            WarnOnce(ref _warnedMissingAudioSource, $"Falta AudioSource en {gameObject.name}.", this);
            return;
        }

        if (ReloadSound == null)
        {
            WarnOnce(ref _warnedMissingReloadSound, $"Falta sonido de recarga/obtencion en {gameObject.name}. Asigna Reload Sound Override o WeaponStats.ReloadSound.", this);
            return;
        }

        _audioSource.PlayOneShot(ReloadSound);
    }

    private void WarnOnce(ref bool hasWarned, string message, Object context)
    {
        if (hasWarned)
            return;

        hasWarned = true;
        Debug.LogWarning(message, context);
    }

    private void ReloadIfEmpty()
    {
        if (currentAmmo > 0 || _isReloading || reserveAmmo <= 0)
            return;

        Reload();
    }

    private bool HasFullAmmo()
    {
        return MagazineSize > 0 && currentAmmo >= MagazineSize;
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
