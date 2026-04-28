using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour, IEnemy
{
    public EnemyStats Stats => _stats;
    [SerializeField] public EnemyStats _stats;

    #region PRIVATE_PROPERTIES
    private float _enemyCurrentLife;
    private float _enemyMaxLife;
    private float _enemySpeed;
    private int _enemyDamage;
    #endregion

    #region IEnemy_PROPERTIES
    public float CurrentLife => _currentLife;
    public float MaxLife => Stats.MaxLife;
    public int Damage => Stats.Damage;
    public float Speed => Stats.MovementSpeed;

    #endregion

    #region IEnemy_METHODS
    public void EnemyAttackDamage(int damage)
    {
        // TODO: Implement attack logic
    }

    public void TakeDamage(int damage)
    {
        _currentLife -= damage;
        if (_currentLife <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    public void Move(Vector3 direction)
    {
        transform.Translate(direction * Speed * Time.deltaTime);
    }
    #endregion

    #region UNITY_EVENTS
    private void Start()
    {
        _enemyCurrentLife = CurrentLife;
        _enemyMaxLife = _enemyMaxLife;
        _enemySpeed = _enemySpeed;
        _enemyDamage = _enemyDamage;
    }

    private void Update()
    {
        // TODO: ver como implementar el movimiento del zombie
    }
    #endregion

}
