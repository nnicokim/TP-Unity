using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class LifeController : MonoBehaviour, IDamagable
{
    #region I_DAMAGABLE_PROPERTIES
    public float CurrentLife => _currentLife;
    [SerializeField] private float _currentLife;

    public float MaxLife => GetComponent<Actor>().Stats.MaxLife;
    #endregion

    #region UNITY_EVENTS
    void Start()
    {
        _currentLife = MaxLife;
        UI_Updater();
    }
    #endregion

    #region I_DAMAGABLE_METHODS
    public void TakeDamage(int damage)
    {
        _currentLife -= damage;
        UI_Updater();

        if (IsDead())
        {
            if (name == "Character") EventManager.instance.EventGameOver(false);
            Die();
        }
    }
    #endregion

    #region PRIVATE_METHODS
    private bool IsDead() => _currentLife <= 0;

    private void Die() => Destroy(this.gameObject);
    #endregion

    public void UI_Updater()
    {
        if (name == "Character") EventManager.instance.CharacterLifeChange(_currentLife, MaxLife);
    }
}
