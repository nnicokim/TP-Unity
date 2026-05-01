using UnityEngine;

public interface IGun
{
    // mesh 
    // sonido 
    // ShootRate 

    GameObject BulletPrefab { get; }
    Transform ParentTransform { get; }
    int Damage { get; }
    int ClipSize { get; }
    int BulletsPerShot { get; }

    void Attack();
    void Reload();
    //void Drop();
}
