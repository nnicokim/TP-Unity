using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class MovementController : MonoBehaviour, IMoveablle
{
    #region IMOVEABLE_PROPERTIES
    public float Speed => GetComponent<Actor>().Stats.MovementSpeed;
    #endregion

    #region IMOVEABLE_METHODS
    public void Move(Vector3 direction)
    {
        transform.position += direction * Time.deltaTime * Speed;
    }
    #endregion
}
