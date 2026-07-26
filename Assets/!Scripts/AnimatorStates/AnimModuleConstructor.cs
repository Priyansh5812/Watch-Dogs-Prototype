using UnityEngine;
using UnityEngine.Events;
[System.Serializable]
public class AnimModuleConstructor
{
    [SerializeField] AnimModuleType moduleType;

    [Min(0f)] public float Threshold;
    public UnityEvent Execution;

    public AnimModuleBase ConstructModule(Animator animator, AnimatorStateInfo info, PlayerStateDriver driver)
    {   
        switch(moduleType)
        {
            case AnimModuleType.ROOT_ENABLED:
                return new Anim_RootMotionEnabled(animator , driver);
            
            case AnimModuleType.ROOT_DISABLED:
                return new Anim_RootMotionDisabled(animator , driver);

            case AnimModuleType.THRESHOLD_EXE:
                return new Anim_ThresholdExecute(info.length , Execution , Threshold);

            case AnimModuleType.VAULT_STATE_COMPLETION:
                return new Anim_VaultStateCompletion(driver);

            case AnimModuleType.MATCH_ROOT_LOCATION:
                return new Anim_MatchRootLocation(animator, driver);

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
    MATCH_ROOT_LOCATION
}