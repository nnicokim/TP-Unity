using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GunStats", menuName = "Stats/Guns", order = 0)]
public class GunStats : ScriptableObject
{
    [SerializeField] private StatsValues _stats;

    public GameObject BulletPrefab => _stats.BulletPrefab;
    public int Damage => _stats.Damage;
    public int MaxBulletCount => _stats.MaxBulletCount;
    public float ShotCooldown => _stats.ShotCooldown;
}

[System.Serializable]
public struct StatsValues
{
    public GameObject BulletPrefab;
    public int Damage;
    public int MaxBulletCount;
    public float ShotCooldown;
}
