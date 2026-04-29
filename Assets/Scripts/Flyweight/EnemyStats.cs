using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Stats/Enemy", order = 0)]
public class EnemyStats : ScriptableObject
{
    [SerializeField] private EnemyStatValues _statValues;

    public int MaxLife => _statValues.MaxLife;
    public int Damage => _statValues.Damage;
    public float MovementSpeed => _statValues.MovementSpeed;
}

[System.Serializable]
public struct EnemyStatValues
{
    public int MaxLife;
    public int Damage;
    public float MovementSpeed;
}
