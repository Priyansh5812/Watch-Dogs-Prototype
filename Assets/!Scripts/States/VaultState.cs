using System;
using System.Threading;
using UnityEngine;
public class VaultState : IPlayerState
{   
    PlayerStateDriver driver;
    CancellationTokenSource src;
    VaultContext ctx;

    Vector3 bkp_CurrentVelocity;

    public VaultState(PlayerStateDriver driver)
    {
        this.driver = driver; 
    }


    public void OnEnter(Action OnCompleted = null)
    {   
        src = new();
        ctx = driver.GetVaultContext();
        PrepareStartup();
        OnCompleted?.Invoke();
    }

    void PrepareStartup()
    {   

        driver.Animator.SetTrigger(ctx.trigger.triggerName);
    }

    public void OnUpdate()
    {
        
    }


    public void FixedUpdate()
    {
        
    }

    public void OnCheckTransition()
    {   
        if(src.IsCancellationRequested)
        {
            driver.InitiateStateChange(ctx.lastStateType);
        }
    }

    public void SetStateCompletion()
    {
        src?.Cancel();
    }

    public void AnimationUpdate()
    {
        
    }

    public void OnExit(Action OnCompleted = null)
    {   
        Time.timeScale = 1f;
        OnCompleted?.Invoke();
    }


}