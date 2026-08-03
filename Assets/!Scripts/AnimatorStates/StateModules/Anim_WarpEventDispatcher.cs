using UnityEngine;

public class Anim_WarpEventDispatcher : AnimModuleBase
{   
    PlayerStateDriver driver;
    AnimatorStateInfo info;
    WarpAsset warpAsset;
    float totalClipTime;
    float startEventTime;
    float endEventTime;
    float currentTime , previousTime;
    VaultState playerState;
    public Anim_WarpEventDispatcher(PlayerStateDriver driver, AnimatorStateInfo info)
    {
        this.driver = driver;
        this.info = info;
        var res = driver.GetVaultContext().trigger;

        if(res != null)
            warpAsset = res.warpAsset;
    }

    public void Refresh()
    {           
        if(warpAsset == null)
            return;

        totalClipTime = warpAsset.TargetClip.length;
        startEventTime = warpAsset.GetStartEventTime();
        endEventTime = warpAsset.GetEndEventTime();
        currentTime = previousTime = 0.0f;
        playerState = driver.GetCurrentState() as VaultState;
    }

    public void Process()
    {
        if(warpAsset == null)
            return;
        
        if(currentTime >= totalClipTime)
            return;

        currentTime += Time.deltaTime;
        if(previousTime < startEventTime && currentTime >= startEventTime)
        {
            // Invoke Event Start
            playerState.OnWarpAreaEntered(startEventTime , endEventTime, currentTime);
        }
        
        if(previousTime < endEventTime && currentTime >= endEventTime)
        {
            // Invoke Event End
            playerState.OnWarpAreaExit();
        }

        previousTime = currentTime;
    }
}