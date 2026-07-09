using System;
using UnityEngine;

public class StateMachineState
{
    protected Zombie enemy;
    protected string animationName;
    protected bool oneshotAnimation = true;
    protected AudioClip[] clips;
    protected bool oneshotAudioclip = true;
    protected StateMachine stateMachine;

    //Total time spent in this state
    protected float elapsedTime;
    //Maximum time (if applicable) to stay in this state
    protected float limitTime = -1f;
    //Cooldown time until a new state switch can occur
    protected float cooldownTime = 0.5f;

    public StateMachineState(Zombie enemy, string animationName, AudioClip[] clips, StateMachine stateMachine)
    {
        this.enemy = enemy;
        this.animationName = animationName;
        this.clips = clips;
        this.stateMachine = stateMachine;
    }

    //Code to run when entering state
    public virtual void Enter()
    {
        elapsedTime = 0f;
        Debug.Log($"Zombie {enemy.name} transitioned to {this.GetType().Name}.");
        enemy.PlayAnimation(animationName, oneshotAnimation);
        enemy.PlayRandomClip(clips, oneshotAudioclip);
    }

    //Code to run when exiting state
    public virtual void Exit()
    {
        if (!oneshotAudioclip) enemy.StopAudioclips();
    }

    //Code to run every frame to update state
    public virtual void UpdateLogic()
    {
        elapsedTime += Time.deltaTime;
    }

}