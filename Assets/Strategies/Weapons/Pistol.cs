using System.Collections;
using UnityEngine;

public class Pistol : Gun
{
    [Header("Muzzle Flash")]
    [SerializeField] private Light muzzleFlashLight;
    [SerializeField, Min(0.01f)] private float muzzleFlashDuration = 0.05f;
    [SerializeField, Min(0f)] private float muzzleFlashIntensity = 8f;
    [SerializeField, Min(0f)] private float muzzleFlashRange = 2f;

    [Header("Pistol Flashlight")]
    [SerializeField] private Light flashlightLight;
    [SerializeField] private bool flashlightEnabled = true;
    [SerializeField, Min(0f)] private float flashlightIntensity = 2.5f;
    [SerializeField, Min(0f)] private float flashlightRange = 12f;
    [SerializeField, Range(1f, 179f)] private float flashlightSpotAngle = 35f;

    private Coroutine _muzzleFlashRoutine;
    private float _defaultMuzzleFlashIntensity;
    private float _defaultMuzzleFlashRange;

    private void Awake()
    {
        ResolveFlashlightLight();
        ResolveMuzzleFlashLight();
        CacheMuzzleFlashDefaults();
        SetMuzzleFlashActive(false);
        ApplyFlashlightSettings();
    }

    private void OnEnable()
    {
        ApplyFlashlightSettings();
    }

    private void OnDisable()
    {
        SetFlashlightActive(false);
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
            muzzleFlashLight = FindNamedLight("muzzle", "flash");

        if (muzzleFlashLight != null && MuzzleTransform != null)
            muzzleFlashLight.transform.position = MuzzleTransform.position;
    }

    private void ResolveFlashlightLight()
    {
        if (flashlightLight == null)
            flashlightLight = FindNamedLight("flashlight", "linterna");
    }

    private Light FindNamedLight(params string[] nameParts)
    {
        Light[] lights = GetComponentsInChildren<Light>(true);
        for (int i = 0; i < lights.Length; i++)
        {
            Light light = lights[i];
            if (light == null)
                continue;

            string lightName = light.name.ToLowerInvariant();
            for (int j = 0; j < nameParts.Length; j++)
            {
                if (lightName.Contains(nameParts[j]))
                    return light;
            }
        }

        return null;
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

    private void ApplyFlashlightSettings()
    {
        if (flashlightLight == null)
            return;

        flashlightLight.type = LightType.Spot;
        flashlightLight.intensity = flashlightIntensity;
        flashlightLight.range = flashlightRange;
        flashlightLight.spotAngle = flashlightSpotAngle;
        SetFlashlightActive(flashlightEnabled);
    }

    private void SetFlashlightActive(bool isActive)
    {
        if (flashlightLight != null)
            flashlightLight.enabled = isActive;
    }
}
