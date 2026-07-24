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
        //Time.timeScale = 0.45f;
        driver.Animator.SetTrigger(ctx.trigger.triggerName);
        if(!ctx.trigger.useLastSpeed)
        {
            Vector3 moveDirection = driver.CurrentVelocity.normalized;
            driver.CurrentVelocity = moveDirection * ctx.trigger.postTriggerSpeed;
        }
        else
        {
            driver.CurrentVelocity = Vector3.ClampMagnitude(driver.CurrentVelocity , ctx.trigger.maxPostTriggerSpeed);
        }

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
            //driver.InitiateStateChange(ctx.lastStateType);

            Type type;
            if(ctx.trigger.postTriggerSpeed <= driver.Data.MaxWalkSpeed)
            {
                type = typeof(WalkState);
            }
            else if(ctx.trigger.postTriggerSpeed <= driver.Data.MaxJogSpeed)
            {
                type = typeof(JogState);
            }
            else
            {
                type = typeof(RunState);
            }

            driver.InitiateStateChange(type);
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