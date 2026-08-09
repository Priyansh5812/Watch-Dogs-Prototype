using UnityEngine;
using System.Buffers;

public class Anim_WarpRuntime : AnimModuleBase
{   
    PlayerStateDriver driver;
    WarpAsset warpAsset;
    float totalClipTime;
    float currentTime , previousTime;
    VaultState playerState;
    VaultContext ctx;

    Vector3 unwarpedPosition;
    Quaternion unwarpedRotation;
    
    Vector3 warpedPosition;
    Quaternion warpedRotation;
    
    Vector3 finalPosition;
    Quaternion finalRotation;
    
    WarpWindow[] activeWarpWindows;
    int validWarpWindowCount;
    


    public Anim_WarpRuntime(PlayerStateDriver driver)
    {
        this.driver = driver;
        ctx = driver.GetVaultContext();
        var res = ctx.trigger;

        if(res != null)
            warpAsset = res.warpAsset;
    }

    public void Refresh()
    {           
        if(warpAsset == null)
            return;

        totalClipTime = warpAsset.TargetClip.length;
        currentTime = previousTime = 0.0f;
        playerState = driver.GetCurrentState() as VaultState;
        finalPosition = warpedPosition = unwarpedPosition = driver.Animator.transform.position;
        finalRotation = warpedRotation = unwarpedRotation = driver.Animator.transform.rotation;
        InitializeReferences();
    }

    void InitializeReferences()
    {
        ReleaseReferences();
        activeWarpWindows = ArrayPool<WarpWindow>.Shared.Rent(10);
        validWarpWindowCount = 0;
    }



    void UpdateTrajectories()
    {
        unwarpedPosition += driver.Animator.deltaPosition;
        unwarpedRotation *= driver.Animator.deltaRotation;

        //-----------

        warpedPosition = unwarpedPosition;
        warpedRotation = unwarpedRotation;
        
        for(int i = 0 ; i < validWarpWindowCount; i++)
        {
            TransformCurves curves = activeWarpWindows[i].curves;
            Vector3 AuthoredValues = activeWarpWindows[i].AuthoredValues;
            Vector3 fromPosition = ctx.traversalPoints[activeWarpWindows[i].Traj_FromIndex];
            Vector3 toPosition = ctx.traversalPoints[activeWarpWindows[i].Traj_ToIndex];

            Vector3 diff = toPosition - fromPosition;
            warpedPosition.x = unwarpedPosition.x * (curves.isXCurveActive ? (Mathf.Abs(diff.x) / AuthoredValues.x) : 1); 
            warpedPosition.y = unwarpedPosition.y * (curves.isYCurveActive ? (Mathf.Abs(diff.y) / AuthoredValues.y) : 1);
            warpedPosition.z = unwarpedPosition.z * (curves.isZCurveActive ? (Mathf.Abs(diff.z) / AuthoredValues.z) : 1);
        }
    }


    void RegisterActiveWarpWindows()
    {
        foreach (var i in warpAsset.windows)
        {
            if (i.IsCurrentTimeUnderBounds(currentTime, totalClipTime))
            {
                activeWarpWindows[validWarpWindowCount++] = i;
            }
        }
    }

    void CalculateFinalTrajectory()
    {
        finalPosition = unwarpedPosition;
        finalRotation = unwarpedRotation;


        for (int i = 0; i < validWarpWindowCount; i++)
        { 
            TransformCurves curves = activeWarpWindows[i].curves;
            Vector3 delta = driver.Animator.deltaPosition;
            

            if(curves.isXCurveActive)
            {   
                float t = curves.XCurve.Evaluate(Mathf.InverseLerp(activeWarpWindows[i].start * totalClipTime , activeWarpWindows[i].end * totalClipTime , currentTime));
                finalPosition.x = Mathf.Lerp(unwarpedPosition.x , warpedPosition.x , t);
            }
            else
            {
                finalPosition.x += delta.x;
            }

            if(curves.isYCurveActive)
            {
                float t = curves.YCurve.Evaluate(Mathf.InverseLerp(activeWarpWindows[i].start * totalClipTime , activeWarpWindows[i].end * totalClipTime , currentTime));
                Debug.Log(t);
                finalPosition.y = Mathf.Lerp(unwarpedPosition.y , warpedPosition.y , t);   
            }
            else
            {
                finalPosition.y += delta.y;
            }

            if(curves.isZCurveActive)
            {
                float t = curves.ZCurve.Evaluate(Mathf.InverseLerp(activeWarpWindows[i].start * totalClipTime , activeWarpWindows[i].end * totalClipTime , currentTime));
                finalPosition.z = Mathf.Lerp(unwarpedPosition.z , warpedPosition.z , t);
            }
            else
            {
                finalPosition.z += delta.z;
            }


        }
        
    }

    public void Process()
    {
        if(warpAsset == null)
            return;
        
        if(currentTime >= totalClipTime)
            return;


        if(!driver.Animator.applyRootMotion)
            return;


        validWarpWindowCount = 0;
        currentTime += Time.deltaTime;

        RegisterActiveWarpWindows();
        UpdateTrajectories();
        CalculateFinalTrajectory();
        Debug.Log(unwarpedPosition);
        Debug.Log(finalPosition);
        playerState?.AnimatorRuntimeCallback(finalPosition, finalRotation);
       
        previousTime = currentTime;
    }

    void ReleaseReferences()
    {
        if (activeWarpWindows != null)
        { 
            ArrayPool<WarpWindow>.Shared.Return(activeWarpWindows);
            activeWarpWindows = null;
        }
    }


    #region Helpers




    #endregion

    public void Dispose()
    {
        ReleaseReferences();
    }
}


public struct WarpTrajectoryBinding
{
    public Vector3 from;
    public Vector3 to;
    public float xMax;
    public float yMax;
    public float zMax;

    public WarpTrajectoryBinding(Vector3 from , Vector3 to)
    {
        this.from = from;
        this.to = to;
        xMax = Mathf.Abs(from.x-to.x);
        yMax = Mathf.Abs(from.y-to.y);
        zMax = Mathf.Abs(from.z-to.z);
    }
}