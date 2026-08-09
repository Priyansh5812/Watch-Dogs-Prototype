using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering.Universal;
public class VaultState : IPlayerState
{   
    PlayerStateDriver driver;
    CancellationTokenSource src;
    VaultContext ctx;
    Transform animatorTransform;
    float startTime;
    float endTime;
    float currentTime;
    bool isWarping; 
    float warpScaleMultiplier;
    float clipLength;
    Vector3 dummyPosition;
    Quaternion startRotation;
    public VaultState(PlayerStateDriver driver)
    {
        this.driver = driver;
        animatorTransform = driver.Animator.transform; 
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
        Time.timeScale = 0.45f;
        driver.CurrentVelocity /= 1.5f;
        driver.Animator.SetTrigger(ctx.trigger.targetValue);
        currentTime = 0;
        clipLength = ctx.trigger.warpAsset.TargetClip.length;
        dummyPosition = driver.Animator.transform.position;
        startRotation = driver.Animator.transform.rotation;
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

    public void AnimatorRuntimeCallback(Vector3 position , Quaternion rotation)
    {   
        if(!driver.Animator.applyRootMotion)
            return;

        animatorTransform.position = position;
        animatorTransform.rotation = rotation;
    }


    public void OnExit(Action OnCompleted = null)
    {   
        Time.timeScale = 1f;
        currentTime = 0;
        OnCompleted?.Invoke();
    }


}