using UnityEngine;
using UnityEngine.Events;
[System.Serializable]
public struct AnimModuleConstructor
{
    [SerializeField] AnimModuleType moduleType;

    [Header("Root Pose Matching")]
    public MatchRootPoseConfig config;

    [Header("Threshold Execute")]
    [Min(0f)] public float Threshold;
    public AnimModuleType postThresholdExecutionType;

    [Header("Targeting")]
    public PrimeTween.Ease easeType;
    public AnchorPointsOverride anchorPointOverride;
    public int startPointIndex;
    public int endPointIndex;
    public bool performHeightAdjustment;

    [Header("Player Driver Simulation")]
    public bool toggle;
    public AnimModuleBase ConstructModule(Animator animator, AnimatorStateInfo info, PlayerStateDriver driver)
    {
        var ctx = driver.GetVaultContext();
        switch (moduleType)
        {
            case AnimModuleType.ROOT_ENABLED:
                return new Anim_RootMotionEnabled(animator , driver);
            
            case AnimModuleType.ROOT_DISABLED:
                return new Anim_RootMotionDisabled(animator , driver);

            case AnimModuleType.THRESHOLD_EXE:
                AnimModuleType temp = moduleType;
                moduleType = postThresholdExecutionType;
                var module = ConstructModule(animator, info , driver);
                moduleType = temp;
                return new Anim_ThresholdExecute(info.length, module ,Threshold);

            case AnimModuleType.VAULT_STATE_COMPLETION:
                return new Anim_VaultStateCompletion(driver);

            case AnimModuleType.MATCH_ROOT_LOCATION:
                return new Anim_MatchRootLocation(animator, driver , config);

            case AnimModuleType.DIRECT_TARGETING:
                return new Anim_TargetLinear(driver, ctx.traversalPoints, info,(startPointIndex , endPointIndex), performHeightAdjustment, easeType, anchorPointOverride);

            case AnimModuleType.DRIVER_SIM_PASS:
                return new Anim_TogglePlayerStateDriverPositioningPass(driver , toggle);
            
            case AnimModuleType.WARP_EVENT_DISP:
                return new Anim_WarpEventDispatcher(driver , info);

            //case AnimModuleType.BEZIER_TARGETING:
            //    return new Anim_TargetBezier(driver, ctx.traversalPoints, info, targetingSpeedMultiplier, easeType, anchorPointType);
            default:
                return null;
        }
    }
}



public enum AnimModuleType
{   
    ROOT_ENABLED,
    ROOT_DISABLED,
    THRESHOLD_EXE,
    VAULT_STATE_COMPLETION,
    MATCH_ROOT_LOCATION,
    BEZIER_TARGETING,
    DIRECT_TARGETING,
    DRIVER_SIM_PASS,
    WARP_EVENT_DISP
}