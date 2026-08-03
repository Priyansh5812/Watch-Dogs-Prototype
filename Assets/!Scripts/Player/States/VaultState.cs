using System;
using System.Threading;
using UnityEngine;
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
        Time.timeScale = 0.85f;
        driver.CurrentVelocity /= 1.5f;
        driver.Animator.SetTrigger(ctx.trigger.targetValue);
        driver._RootMotionRuntime.AddRuntime(AnimatorRuntimeCallback);
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

    void AnimatorRuntimeCallback()
    {   
        if(!driver.Animator.applyRootMotion)
            return;
        
        Vector3 deltaPosition = driver.Animator.deltaPosition;

        if (isWarping)
        { 
            float multiplier = Mathf.Lerp(1, warpScaleMultiplier, ctx.trigger.warpAsset.EvaulateWarpScaleCurve(Mathf.InverseLerp(startTime, endTime, currentTime), UnityEngine.Animations.Axis.Y));
            deltaPosition.y *= multiplier;
            currentTime += Time.deltaTime;
        }

        animatorTransform.position += deltaPosition;
        animatorTransform.rotation *= driver.Animator.deltaRotation;
    }

    public void OnWarpAreaEntered(float startTime, float endTime, float currentTime)
    {
        this.startTime = startTime;
        this.endTime = endTime; 
        this.currentTime = currentTime;
        warpScaleMultiplier = Mathf.Abs(ctx.traversalPoints[1].y - ctx.traversalPoints[0].y) / ctx.trigger.warpAsset.GetMaxOffset(UnityEngine.Animations.Axis.Y);
        isWarping = true;
        Debug.Log("Warping Started : " + warpScaleMultiplier);
    }

    public void OnWarpAreaExit()
    {
        isWarping = false;
        Debug.Log("Warping Ended");
    }

    public void OnExit(Action OnCompleted = null)
    {   
        Time.timeScale = 1f;
        driver._RootMotionRuntime.RemoveRuntime(AnimatorRuntimeCallback);
        OnCompleted?.Invoke();
    }


}