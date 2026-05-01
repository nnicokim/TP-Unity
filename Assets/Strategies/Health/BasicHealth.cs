using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class BasicHealth : Actor, IDamageable
{
    public int Life => _life;
    [SerializeField] private int _life;

    public int MaxLife => Stats.MaxLife;

    private void Start()
    {
        Debug.Log($"MaxLife stat: {Stats.MaxLife}");
        SetLife();
    }

    public virtual void ApplyDamage(int damage)
    {
        if (gameObject == null)
        {
            Debug.LogError($"El objeto ya ha sido destruido.");
            return;
        }
        _life -= damage;
        if (_life <= 0) Die();
    }

    public virtual void ApplyHealthRecovery(int amount) => _life = (_life + amount) >= MaxLife ? MaxLife : _life + amount;

    public virtual void Die()
    {
        if (gameObject == null)
        {
            Debug.LogError($"El objeto ya ha sido destruido.");
            return;
        }
        Destroy(gameObject);
        Debug.Log($"Objeto {name} ha muerto!!!");
    }

    private void OnDestroy() { } // => Debug.Log($"Objeto {name} ha muerto!!!");

    protected void SetLife() => _life = MaxLife;
}
