using System.Collections;
using UnityEngine;

public class PlayerHealth : BasicHealth
{
    private bool _lostGame = false;

    public override void ApplyDamage(int damage)
    {
        base.ApplyDamage(damage);
        LifeUiFeedback();
    }

    public override void ApplyHealthRecovery(int amount)
    {
        base.ApplyHealthRecovery(amount);
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
