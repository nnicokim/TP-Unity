using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    [SerializeField] private ActorStats _stats;
    public ActorStats Stats => _stats;
}
