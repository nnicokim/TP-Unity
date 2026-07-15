using System.Collections;
using UnityEngine;

public class PlayerHealth : BasicHealth
{
    private bool _lostGame = false;

    public override void ApplyDamage(int damage, DamageType type)
    {
        int lifeBeforeDamage = Mathf.Max(0, Life);

        base.ApplyDamage(damage, type);

        int lifeAfterDamage = Mathf.Max(0, Life);
        GameplayStatsManager.RegisterDamageTaken(lifeBeforeDamage - lifeAfterDamage);

        LifeUiFeedback();
    }

    public override void ApplyHealthRecovery(int amount)
    {
        int lifeBeforeHeal = Life;

        base.ApplyHealthRecovery(amount);

        GameplayStatsManager.RegisterLifeHealed(Life - lifeBeforeHeal);

        LifeUiFeedback();
    }

    public override void Die()
    {
        ActionsManager.instance.ActionGameover(_lostGame);

        Debug.Log("The player is dead!!!");
    }

    private void Start()
    {
        base.SetLife();
        StartCoroutine(LateStart());
    }

    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(.25f);

        LifeUiFeedback();
    }

    private void LifeUiFeedback()
    {
        if (ActionsManager.instance != null)
            ActionsManager.instance.ActionLifeFeedback(Life, MaxLife);
    }
}
