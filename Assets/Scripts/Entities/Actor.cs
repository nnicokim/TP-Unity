using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    public ActorStats Stats => _stats;
    [SerializeField] public ActorStats _stats;
}
