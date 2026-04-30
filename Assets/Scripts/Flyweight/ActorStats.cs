using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActorStats", menuName = "Stats/Actor", order = 0)]
public class ActorStats : ScriptableObject
{
    [SerializeField] private ActorStruct _struct;

    public int MaxLife => _struct.MaxLife;
    public float MoveSpeed => _struct.MoveSpeed;
    public float RunSpeed => _struct.RunSpeed;
    public float RotationSpeed => _struct.RotationSpeed;
}

[System.Serializable]
public struct ActorStruct
{
    public int MaxLife;
    public float MoveSpeed;
    public float RunSpeed;
    public float RotationSpeed;
}
