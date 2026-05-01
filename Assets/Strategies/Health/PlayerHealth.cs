using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerHealth : BasicHealth
{
    private bool _lostGame = false;
    public override void ApplyDamage(int damage)
    {
        base.ApplyDamage(damage);
        //ActionsManager.instance.ActionLifeFeedback(Life, MaxLife);
    }

    public override void ApplyHealthRecovery(int amount)
    {
        base.ApplyHealthRecovery(amount);
        //ActionsManager.instance.ActionLifeFeedback(Life, MaxLife);
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

        //ActionsManager.instance.ActionLifeFeedback(MaxLife, MaxLife);
    }

}
