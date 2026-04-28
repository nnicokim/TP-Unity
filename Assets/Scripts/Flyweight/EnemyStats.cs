using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Stats/Enemy", order = 0)]
public class EnemyStats : ScriptableObject
{
    [SerializeField] private StatValues _statValues;

    public int MaxLife => _statValues.MaxLife;
    public int Damage => _statValues.Damage;
    public float MovementSpeed => _statValues.MovementSpeed;
}

[System.Serializable]
public struct StatValues
{
    public int MaxLife;
    public int Damage;
    public float MovementSpeed;
}
