using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActorStats", menuName = "Stats/Actor", order = 0)]
public class ActorStats : ScriptableObject
{
    [SerializeField] private ActorStatValues _statValues;

    public int MaxLife => _statValues.MaxLife;
    public float WalkSpeed => _statValues.WalkSpeed;
    public float RunSpeed => _statValues.RunSpeed;
    public float RotationSpeed => _statValues.RotationSpeed;
}

[System.Serializable]
public struct ActorStatValues
{
    public int MaxLife;
    public float WalkSpeed;
    public float RunSpeed;
    public float RotationSpeed;
}
