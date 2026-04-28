using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour, IGun
{
    [SerializeField] protected GunStats _stats;

    #region GUN_PROPERTIES
    [SerializeField] protected int _currentBulletCount;
    [SerializeField] protected float _currentShotCooldown;
    #endregion

    #region I_GUN_PROPERTIES
    public GameObject BulletPrefab => _stats.BulletPrefab;
    public int Damage => _stats.Damage;
    public int MaxBulletCount => _stats.MaxBulletCount;
    public float ShotCooldown => _stats.ShotCooldown;
    #endregion

    #region UNITY_EVENTS
    private void Start()
    {
        _currentBulletCount = MaxBulletCount;
        _currentShotCooldown = ShotCooldown;

        UI_Updater();
    }

    private void Update()
    {
        if (_currentShotCooldown >= 0) _currentShotCooldown -= Time.deltaTime;
    }
    #endregion

    #region I_GUN_PROPERTIES
    public virtual void Attack()
    {
        if (_currentShotCooldown <= 0 && _currentBulletCount > 0)
        {
            var bullet = Instantiate(BulletPrefab, transform.position, transform.rotation);
            bullet.GetComponent<Bullet>().SetOwner(this);

            _currentShotCooldown = ShotCooldown;
            _currentBulletCount--;

            UI_Updater();
        }
    }

    public virtual void Reload()
    {
        _currentBulletCount = MaxBulletCount;
        UI_Updater();
    }
    #endregion

    public void UI_Updater() => EventManager.instance.AmmoChange(_currentBulletCount, _stats.MaxBulletCount);
}
