using System.Collections;
using UnityEngine;

public class Pistol : Gun
{
    [Header("Muzzle Flash")]
    [SerializeField] private Light muzzleFlashLight;
    [SerializeField, Min(0.01f)] private float muzzleFlashDuration = 0.05f;
    [SerializeField, Min(0f)] private float muzzleFlashIntensity = 8f;
    [SerializeField, Min(0f)] private float muzzleFlashRange = 2f;

    private Coroutine _muzzleFlashRoutine;
    private float _defaultMuzzleFlashIntensity;
    private float _defaultMuzzleFlashRange;

    private void Awake()
    {
        ResolveMuzzleFlashLight();
        CacheMuzzleFlashDefaults();
        SetMuzzleFlashActive(false);
    }

    public override void Attack()
    {
        if (CanShoot)
        {
            Vector3 spawnPosition = MuzzlePosition;
            Quaternion shootRotation = GetShootRotation(spawnPosition);
            CreateBullet(spawnPosition, shootRotation);
            _bulletCount--;
            PlayMuzzleFlash();
            base.Attack();
        }
    }

    private void ResolveMuzzleFlashLight()
    {
        if (muzzleFlashLight == null)
            muzzleFlashLight = GetComponentInChildren<Light>(true);

        if (muzzleFlashLight != null && MuzzleTransform != null)
            muzzleFlashLight.transform.position = MuzzleTransform.position;
    }

    private void CacheMuzzleFlashDefaults()
    {
        if (muzzleFlashLight == null)
            return;

        _defaultMuzzleFlashIntensity = muzzleFlashLight.intensity;
        _defaultMuzzleFlashRange = muzzleFlashLight.range;
    }

    private void PlayMuzzleFlash()
    {
        if (muzzleFlashLight == null)
            return;

        if (_muzzleFlashRoutine != null)
            StopCoroutine(_muzzleFlashRoutine);

        _muzzleFlashRoutine = StartCoroutine(MuzzleFlashRoutine());
    }

    private IEnumerator MuzzleFlashRoutine()
    {
        muzzleFlashLight.intensity = muzzleFlashIntensity;
        muzzleFlashLight.range = muzzleFlashRange;
        SetMuzzleFlashActive(true);

        yield return new WaitForSeconds(muzzleFlashDuration);

        muzzleFlashLight.intensity = _defaultMuzzleFlashIntensity;
        muzzleFlashLight.range = _defaultMuzzleFlashRange;
        SetMuzzleFlashActive(false);
        _muzzleFlashRoutine = null;
    }

    private void SetMuzzleFlashActive(bool isActive)
    {
        if (muzzleFlashLight != null)
            muzzleFlashLight.enabled = isActive;
    }
}
