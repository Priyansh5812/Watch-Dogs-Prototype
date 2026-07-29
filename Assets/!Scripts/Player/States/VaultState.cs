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
        Debug.Log("Entered Vault State");
        src = new();
        ctx = driver.GetVaultContext();
        PrepareStartup();
        OnCompleted?.Invoke();
    }

    void PrepareStartup()
    {   
        //Time.timeScale = 0.45f;
        driver.CurrentVelocity /= 1.5f;
        driver.Animator.SetTrigger(ctx.trigger);
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
        driver.Animator.SetFloat("Loco" , driver.CurrentVelocity.magnitude / driver.Data.MaxRunSpeed);
    }

    public void OnExit(Action OnCompleted = null)
    {   
        Time.timeScale = 1f;
        OnCompleted?.Invoke();
    }


}