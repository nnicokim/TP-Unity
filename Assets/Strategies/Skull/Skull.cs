using UnityEngine;

[DisallowMultipleComponent]
public class Skull : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField, Min(1)] private int maxLife = 30;
    [SerializeField, Min(0)] private int life;

    [Header("Damage Bonus")]
    [SerializeField, Min(0f)] private float bonusDuration = 10f;
    [SerializeField] private bool destroyOnDeath = true;

    private bool _isDead;

    public int Life => life;
    public int MaxLife => maxLife;

    private void Awake()
    {
        life = Mathf.Clamp(life > 0 ? life : maxLife, 0, maxLife);
    }

    public void ApplyDamage(int damage, DamageType type)
    {
        if (_isDead)
            return;

        life = Mathf.Max(0, life - Mathf.Max(0, damage));
        if (life <= 0)
            Die();
    }

    public void ApplyHealthRecovery(int amount)
    {
        if (_isDead)
            return;

        life = Mathf.Clamp(life + Mathf.Max(0, amount), 0, maxLife);
    }

    public void Die()
    {
        if (_isDead)
            return;

        _isDead = true;
        DamageBonusManager.ActivateDamageBonus(bonusDuration);

        if (destroyOnDeath)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }
}
