using UnityEngine;

[CreateAssetMenu(fileName = "WeaponStats", menuName = "Stats/Weapon", order = 0)]
public class WeaponStats : ScriptableObject
{
    [SerializeField] private WeaponStruct _struct;

    public GameObject BulletPrefab => _struct.BulletPrefab;
    public int Damage => _struct.Damage;
    public int ClipSize => _struct.ClipSize;
    public int BulletsPerShot => _struct.BulletsPerShot;
    public float BulletMaxPositionRadius => _struct.BulletMaxPositionRadius;
    public float BulletMaxRandomAngle => _struct.BulletMaxRandomAngle;
    public float BulletReloadTime => _struct.ReloadTime;
    public AudioClip ShotSound => _struct.ShotSound;
    public AudioClip ReloadSound => _struct.ReloadSound;
}

[System.Serializable]
public struct WeaponStruct
{
    public GameObject BulletPrefab;
    public int Damage;
    public int ClipSize;
    public int BulletsPerShot;
    public AudioClip ShotSound;
    public AudioClip ReloadSound;
    public float BulletMaxPositionRadius;
    public float BulletMaxRandomAngle;
    public float ReloadTime;
}
