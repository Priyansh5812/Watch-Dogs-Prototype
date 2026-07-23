using UnityEngine;

public class Anim_Vault : AnimStateAbstract
{
    
    [SerializeField] VaultType vaultType;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
        
    }


    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        VaultState state = driver.GetCurrentState() as VaultState;

        if(state == null)
        {
            Debug.LogError("State is not in Vault State while performing vault animation!!!");
            return;
        }

        state.SetStateCompletion();
    }

}

public enum VaultType
{
    SLIDE_OVER,
    JUMP_OVER,
    ROLL_OVER
}