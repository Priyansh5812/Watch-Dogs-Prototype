using UnityEditor.Rendering;
using UnityEngine;

public class Anim_Walk180 : StateMachineBehaviour
{
    PlayerStateDriver driver;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        RefreshDriverModule(animator);
        animator.gameObject.transform.SetParent(null);
        driver.IsUnderRootRotation = true;
        animator.applyRootMotion = true;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
        driver.transform.position = animator.gameObject.transform.position;
    }

    

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        RefreshDriverModule(animator);
        animator.gameObject.transform.SetParent(driver.transform);
        driver.IsUnderRootRotation = false;
        animator.applyRootMotion = false;
    }

    void RefreshDriverModule(Animator animator)
    {
        if(driver == null)
        {
            driver = animator.gameObject.transform.parent.GetComponent<PlayerStateDriver>();
        }
    }

}   
