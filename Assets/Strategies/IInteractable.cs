using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    int Value { get; }
    void Interact(Collider Collider);
}
