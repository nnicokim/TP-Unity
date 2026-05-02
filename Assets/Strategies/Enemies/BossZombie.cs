using UnityEngine;

public class BossZombie : Zombie
{
    [SerializeField] private ExitDoor _exitDoor;

    protected override void OnDie()
    {
        if (_exitDoor != null)
            _exitDoor.Unlock();
    }
}
